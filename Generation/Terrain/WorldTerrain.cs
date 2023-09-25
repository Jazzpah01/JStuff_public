using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using JStuff.Threading;
using Debug = UnityEngine.Debug;
using System.Linq;

namespace JStuff.Generation.Terrain
{
    public class WorldTerrain : MonoBehaviour
    {
        // These are for the Aljeja project only, please remove
        public static WorldTerrain instance;
        // These are for the Aljeja project only, please remove

        [Header("Graph Settings")]
        public TerrainGraph graphData;
        public Material material;
        public string blockLayer = "Default";
        public bool raymarchingTerrain = false;
        public bool enableThreading = false;

        [Header("Visual Settings")]
        public float generateDistance = 100;
        public float generateDistanceEditor = 1000;
        [Min(1)] public int terrainHalfsize = 50;
        public Transform cameraTransform;

        public TerrainLODSettings LODSettings;
        public FoliageLODSettings foliageLODSettings;

        public int colliderLOD = 2;
        public float colliderAtDistance = 200;

        [Header("Debug")]
        public TerrainGraph initializedGraph;
        public bool debugTime = false;
        public List<Block> blocks;
        private List<Block> usedBlocks;
        private Stack<Block> depricatedBlocks;
        public GameObject parentBlock;
        public GameObject savedBlocksParent;

        // Coordinate to block
        public Dictionary<TerrainCoordinate, Block> blockOfCoordinates;

        private float updateOnCameraChangeDistance = 150f;

        private Block[] savedBlocks;

        Queue<Vector3> newPositions;

        Vector3 centerBlock;

        private HashSet<TerrainCoordinate> savedCoordinates;

        private HashSet<Vector3> oldPositions;

        [HideInInspector]public float blockSize;
        [HideInInspector]public float scale;
        [HideInInspector]public int seed;
        [HideInInspector]public float zoom;

        bool shouldUpdate = false;

        float maxWorkLoad;


        System.Diagnostics.Stopwatch stopwatch = new Stopwatch();

        public bool IsUpdating
        {
            get
            {
                if (blocks == null)
                    return false;
                foreach (Block block in blocks)
                {
                    if (block.waitingJobResult || block.waitingCoroutine)
                        return true;
                }
                return false;
            }
        }

        public void Initialize()
        {
            initializedGraph = graphData.GetInitializedGraph() as TerrainGraph;

            blockSize = graphData.chunkSize;
            scale = graphData.scale;
            seed = graphData.seed;
            zoom = graphData.zoom;
            depricatedBlocks = new Stack<Block>();
            oldPositions = new HashSet<Vector3>();
            blocks = new List<Block>();
            usedBlocks = new List<Block>();

            maxWorkLoad = LODSettings.worldTerrainMaxWorkload_Ms;
            generateDistance = LODSettings.worldTerrainDistance;

            savedCoordinates = new HashSet<TerrainCoordinate>();

            if (savedBlocksParent == null)
                savedBlocksParent = new GameObject();

            savedBlocks = savedBlocksParent.GetComponentsInChildren<Block>();

            foreach (Block block in savedBlocks)
            {
                savedCoordinates.Add(block.GetCoordinates());
            }
        }

        public void SaveBlock(Block block)
        {
            if (savedBlocksParent == null)
            {
                savedBlocksParent = new GameObject("Saved Blocks Parent");
            }

            if (savedBlocks == null || savedBlocks.Length == 0)
            {
                savedBlocks = savedBlocksParent.GetComponentsInChildren<Block>();
            }

            bool savedcontains = false;
            for (int i = 0; i < savedBlocks.Length; i++)
            {
                if (savedBlocks[i].name == block.name)
                {
                    savedcontains = true;
                    if (savedBlocks[i] != block)
                    {
                        Block tmp = savedBlocks[i];
                        savedBlocks[i] = block;
                        DestroyImmediate(tmp.gameObject);
                    }
                }
            }

            block.gameObject.transform.parent = savedBlocksParent.transform;
        }

        private void Awake()
        {
            instance = this;
            updateOnCameraChangeDistance = LODSettings.worldTerrainUpdateDistance;
        }

        private void Start()
        {
            Cleanup();

            if (cameraTransform == null)
                cameraTransform = Camera.main.transform;

            transform.position = new Vector3(cameraTransform.position.x, 0, cameraTransform.position.z);

            Initialize();

            blockOfCoordinates = new Dictionary<TerrainCoordinate, Block>();

            foreach (Block block in savedBlocks)
            {
                savedCoordinates.Add(block.GetCoordinates()); // This is done in initialize too, double done???
                blockOfCoordinates.Add(block.GetCoordinates(), block);
            }

            if (raymarchingTerrain)
            {
                material.SetFloat("Scale", scale);
                material.SetFloat("Stretch", blockSize * zoom);
            }
            
            
            if (debugTime)
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                Populate(GetCenterChunkPosition());
                UpdateAll(centerBlock);
                JobManagerComponent.instance.manager.FinishJobs();
                watch.Stop();
                Debug.Log("Populating time: " + watch.Elapsed);
            } else
            {
                Populate(GetCenterChunkPosition());
                UpdateAll(centerBlock);
                JobManagerComponent.instance.manager.FinishJobs();
            }

            JobManagerComponent.instance.manager.ConsumeAll();
        }
#if UNITY_EDITOR
        public void GenerateEditorTerrain(Vector3 position)
        {
            Cleanup();
            Initialize();

            position.y = 0;
            transform.position = position;
            
            Populate(GetCenterChunkPosition());

            EnqueuePositions(GetCenterChunkPosition(), generateDistanceEditor);

            while (newPositions.Count > 0)
            {
                Vector3 p = newPositions.Dequeue();

                if (savedCoordinates.Contains(new TerrainCoordinate(blockSize, p)))
                    continue;

                Block b = NewBlock();

                b.UpdateValues(p, p);
                b.EditorGenerateRenderMesh();
            }
        }
#endif
        public void Cleanup()
        {
            if (parentBlock != null)
            {
                DestroyImmediate(parentBlock);
                parentBlock = null;
            }

            if (savedBlocksParent == null)
            {
                savedBlocksParent = null;
            }

            savedBlocks = null;
            blocks = null;
        }

        Coroutine coroutine;
        bool coroutineInProgress = false;
        Vector3 newCenterChunk;

        public enum UpdateState
        {
            None,
            EnqueuePositions,
            DepricatedBlocks,
            SetBlockPositions,
            UpdateBlocks,
        }

        UpdateState currentUpdateState = UpdateState.None;
        int i, j;
        HashSet<Block> updated = new HashSet<Block>();

        private void Update()
        {
            stopwatch.Restart();

            if ((new Vector3(cameraTransform.position.x, 0, cameraTransform.position.z) - transform.position).magnitude > updateOnCameraChangeDistance && !shouldUpdate && JobManagerComponent.instance.manager.Pending == 0)
            {
                transform.position = new Vector3(cameraTransform.position.x, 0, cameraTransform.position.z);
                shouldUpdate = true;
            }

            Vector3 newCenterChunk = transform.position - new Vector3(transform.position.x % blockSize, 0, transform.position.z % blockSize);

            if (shouldUpdate)
            {
                shouldUpdate = false;
                coroutineInProgress = true;
                if (JobManagerComponent.instance.manager.Pending != 0)
                {
                    Debug.Log("Running FinishedJobs!");
                    JobManagerComponent.instance.manager.FinishJobs();
                }

                StartCoroutine(UpdateAllCoroutine(newCenterChunk));
                centerBlock = newCenterChunk;
            }
        }

        //private void Update()
        //{
        //    stopwatch.Restart();

        //    switch (currentUpdateState)
        //    {
        //        case UpdateState.None:
        //            if ((new Vector3(cameraTransform.position.x, 0, cameraTransform.position.z) - transform.position).magnitude > updateOnCameraChangeDistance && !shouldUpdate && JobManagerComponent.instance.manager.Pending == 0)
        //            {
        //                transform.position = new Vector3(cameraTransform.position.x, 0, cameraTransform.position.z);
        //                shouldUpdate = true;
        //            }

        //            Vector3 newCenterChunk = transform.position - new Vector3(transform.position.x % blockSize, 0, transform.position.z % blockSize);

        //            if (shouldUpdate)
        //            {
        //                shouldUpdate = false;
        //                coroutineInProgress = true;
        //                if (JobManagerComponent.instance.manager.Pending != 0)
        //                {
        //                    Debug.Log("Running FinishedJobs!");
        //                    JobManagerComponent.instance.manager.FinishJobs();
        //                }

        //                currentUpdateState = UpdateState.EnqueuePositions;
        //                this.newCenterChunk = newCenterChunk;
        //                centerBlock = newCenterChunk;
        //                i = 0;
        //            }
        //            break;
        //        case UpdateState.EnqueuePositions:
        //            int s = (int) Mathf.Pow((terrainHalfsize + 1) * 2, 2);
        //            while (i < s)
        //            {
        //                int x = i % s - (int)(s / 2);
        //                int y = i / s - (int)(s / 2);

        //                Vector3 pos = this.newCenterChunk + new Vector3(x, 0, y) * blockSize;

        //                if (!oldPositions.Contains(pos) && (pos - this.newCenterChunk).magnitude <= generateDistance)
        //                {
        //                    newPositions.Enqueue(pos);
        //                }
        //                i++;

        //                if (stopwatch.ElapsedMilliseconds > 2)
        //                {
        //                    if (stopwatch.ElapsedMilliseconds > 150)
        //                    {
        //                        Debug.Log($"Big update!!!?! UpdateState.EnqueuePositions: {stopwatch.ElapsedMilliseconds}");
        //                    }
        //                    return;
        //                }
        //            }

        //            currentUpdateState = UpdateState.DepricatedBlocks;
        //            i = 0;
        //            break;
        //        case UpdateState.DepricatedBlocks:
        //            // Get depricated blocks
        //            while (i < usedBlocks.Count)
        //            {
        //                if (Vector3.Distance(usedBlocks[i].transform.position, this.newCenterChunk) >= generateDistance)
        //                {
        //                    oldPositions.Remove(usedBlocks[i].targetPosition);
        //                    depricatedBlocks.Push(usedBlocks[i]);
        //                    usedBlocks.Remove(usedBlocks[i]);
        //                    i--;
        //                }
        //                i++;

        //                if (stopwatch.ElapsedMilliseconds > 2)
        //                {
        //                    if (stopwatch.ElapsedMilliseconds > 150)
        //                    {
        //                        Debug.Log($"Big update!!!?! UpdateState.DepricatedBlocks: {stopwatch.ElapsedMilliseconds}");
        //                    }
        //                    return;
        //                }
        //            }

        //            currentUpdateState = UpdateState.SetBlockPositions;
        //            updated = new HashSet<Block>();
        //            break;
        //        case UpdateState.SetBlockPositions:
        //            // Match blocks with new positions
        //            while (newPositions.Count > 0)
        //            {
        //                Vector3 p = newPositions.Dequeue();

        //                Block b = null;

        //                if (depricatedBlocks.Count > 0)
        //                {
        //                    b = depricatedBlocks.Pop();
        //                }
        //                else
        //                {
        //                    b = NewBlock();
        //                }

        //                usedBlocks.Add(b);

        //                oldPositions.Add(p);

        //                UpdateBlock(b, this.newCenterChunk, p);
        //                updated.Add(b);

        //                if (stopwatch.ElapsedMilliseconds > 2)
        //                {
        //                    if (stopwatch.ElapsedMilliseconds > 150)
        //                    {
        //                        Debug.Log($"Big update!!!?! UpdateState.SetBlockPositions: {stopwatch.ElapsedMilliseconds}");
        //                    }
        //                    return;
        //                }
        //            }

        //            currentUpdateState = UpdateState.UpdateBlocks;
        //            i = 0;
        //            break;
        //        case UpdateState.UpdateBlocks:
        //            // Update all blocks
        //            while (i < blocks.Count)
        //            {
        //                Block b = blocks[i];
        //                if (!updated.Contains(b))
        //                    UpdateBlock(b, this.newCenterChunk, b.targetPosition);
        //                i++;

        //                if (stopwatch.ElapsedMilliseconds > 2)
        //                {
        //                    if (stopwatch.ElapsedMilliseconds > 150)
        //                    {
        //                        Debug.Log($"Big update!!!?! UpdateState.UpdateBlocks: {stopwatch.ElapsedMilliseconds}");
        //                    }
        //                    return;
        //                }
        //            }

        //            currentUpdateState = UpdateState.None;
        //            centerBlock = this.newCenterChunk;
        //            break;
        //    }

        //    if (stopwatch.ElapsedMilliseconds > 150)
        //    {
        //        Debug.Log($"Big update!!!?!: {stopwatch.ElapsedMilliseconds}");
        //    }
        //}

        public Block NewBlock()
        {
            GameObject go = new GameObject("Block" + (blocks.Count));
            go.layer = LayerMask.NameToLayer(blockLayer);
            go.transform.parent = parentBlock.transform;
            go.transform.position = new Vector3(-10000, -1000000, -1000);//centerBlock + new Vector3(i - terrainHalfsize, 0, j - terrainHalfsize) * blockSize;
                                                                         //go.transform.position = new Vector3(30 * chunkSize, 0, 30 * chunkSize);
            Block c = go.AddComponent<Block>();
            c.Initialize(this, (TerrainGraph)initializedGraph.GetChild(), material);
            blocks.Add(c);
            return c;
        }

        public void Populate(Vector3 centerChunkPos)
        {
            Cleanup();

            int chunkAmount = terrainHalfsize * 2 + 1;
            blocks = new List<Block>();
            newPositions = new Queue<Vector3>();

            centerBlock = transform.position - new Vector3(transform.position.x % blockSize, 0, transform.position.z % blockSize);

            parentBlock = new GameObject("Blocks");
        }

        public void EnqueuePositions(Vector3 newCenterChunk, float generateDistance)
        {
            // Big optimisation missing
            for (int i = -terrainHalfsize; i < terrainHalfsize + 1; i++)
            {
                for (int j = -terrainHalfsize; j < terrainHalfsize + 1; j++)
                {
                    //newPositions.Enqueue(newCenterChunk + new Vector3(i - Mathf.Sqrt(blocks.Length) / 2, 0, j - Mathf.Sqrt(blocks.Length) / 2) * chunkSize);
                    Vector3 pos = newCenterChunk + new Vector3(i, 0, j) * blockSize;

                    if (!oldPositions.Contains(pos) && (pos - newCenterChunk).magnitude <= generateDistance)
                    {
                        newPositions.Enqueue(pos);
                    }
                }
            }
        }

        public void EnqueueDepricatedBlocks(Vector3 newCenterChunk)
        {
            for (int i = 0; i < usedBlocks.Count; i++)
            {
                if (Vector3.Distance(usedBlocks[i].transform.position, newCenterChunk) >= generateDistance)
                {
                    oldPositions.Remove(usedBlocks[i].targetPosition);
                    depricatedBlocks.Push(usedBlocks[i]);
                    usedBlocks.Remove(usedBlocks[i]);
                    i--;
                }
            }
        }

        public void UpdateAll(Vector3 newCenterChunk)
        {
            EnqueuePositions(newCenterChunk, generateDistance);
            EnqueueDepricatedBlocks(newCenterChunk);

            HashSet<Block> updated = new HashSet<Block>();

            Debug.Log("New positions amound: " + newPositions.Count);
            Debug.Log("Depricated Blocks: " + depricatedBlocks.Count);

            while (newPositions.Count > 0)
            {
                Vector3 p = newPositions.Dequeue();

                Block b = null;

                if (depricatedBlocks.Count > 0)
                {
                    b = depricatedBlocks.Pop();
                } else
                {
                    b = NewBlock();
                }

                usedBlocks.Add(b);

                oldPositions.Add(p);

                UpdateBlock(b, newCenterChunk, p);
                updated.Add(b);
            }

            foreach (Block b in blocks)
            {
                if (!updated.Contains(b))
                    UpdateBlock(b, newCenterChunk, b.targetPosition);
            }

            centerBlock = newCenterChunk;
        }



        public IEnumerator UpdateAllCoroutine(Vector3 newCenterChunk)
        {
            bool halted = false;
            stopwatch.Restart();

            // Big optimisation missing
            for (int i = -terrainHalfsize; i < terrainHalfsize + 1; i++)
            {
                for (int j = -terrainHalfsize; j < terrainHalfsize + 1; j++)
                {
                    if (halted)
                    {
                        halted = false;
                        stopwatch.Restart();
                    }
                    else if (stopwatch.ElapsedMilliseconds > maxWorkLoad)
                    {
                        halted = true;
                        yield return null;
                    }

                    //newPositions.Enqueue(newCenterChunk + new Vector3(i - Mathf.Sqrt(blocks.Length) / 2, 0, j - Mathf.Sqrt(blocks.Length) / 2) * chunkSize);
                    Vector3 pos = newCenterChunk + new Vector3(i, 0, j) * blockSize;

                    if (!oldPositions.Contains(pos) && (pos - newCenterChunk).magnitude <= generateDistance)
                    {
                        newPositions.Enqueue(pos);
                    }
                }
            }


            for (int i = 0; i < usedBlocks.Count; i++)
            {
                if (halted)
                {
                    halted = false;
                    stopwatch.Restart();
                }
                else if (stopwatch.ElapsedMilliseconds > maxWorkLoad)
                {
                    halted = true;
                    yield return null;
                }

                if (Vector3.Distance(usedBlocks[i].transform.position, newCenterChunk) >= generateDistance)
                {
                    oldPositions.Remove(usedBlocks[i].targetPosition);
                    depricatedBlocks.Push(usedBlocks[i]);
                    usedBlocks.Remove(usedBlocks[i]);
                    i--;
                }
            }

            HashSet<Block> updated = new HashSet<Block>();

            Debug.Log("New positions amound: " + newPositions.Count);
            Debug.Log("Depricated Blocks: " + depricatedBlocks.Count);

            while (newPositions.Count > 0)
            {
                if (halted)
                {
                    halted = false;
                    stopwatch.Restart();
                } else if (stopwatch.ElapsedMilliseconds > maxWorkLoad)
                {
                    halted = true;
                    yield return null;
                }

                Vector3 p = newPositions.Dequeue();

                Block b = null;

                if (depricatedBlocks.Count > 0)
                {
                    b = depricatedBlocks.Pop();
                }
                else
                {
                    b = NewBlock();
                }

                usedBlocks.Add(b);

                oldPositions.Add(p);

                UpdateBlock(b, newCenterChunk, p);
                updated.Add(b);
            }

            foreach (Block b in blocks)
            {
                if (halted)
                {
                    halted = false;
                    stopwatch.Restart();
                }
                else if (stopwatch.ElapsedMilliseconds > maxWorkLoad)
                {
                    halted = true;
                    yield return null;
                }

                if (!updated.Contains(b))
                    UpdateBlock(b, newCenterChunk, b.targetPosition);
            }

            centerBlock = newCenterChunk;

            coroutineInProgress = false;
        }

        public void UpdateBlock(Block b, Vector3 newCenterChunk, Vector3 newpos)
        {
            if (savedCoordinates.Contains(new TerrainCoordinate(blockSize, newpos)))
            {
                b.gameObject.SetActive(false);
                return;
            } else
            {
                b.gameObject.SetActive(true);
            }

            b.UpdateBlock(newCenterChunk, newpos);
        }

        private void DynamicUpdate(Vector3 newCenterChunk, Vector3 oldCenterChunk)
        {
            //int x_diff = Mathf.RoundToInt(Mathf.Abs((oldCenterChunk - newCenterChunk).x)/chunkSize);
            //int y_diff = Mathf.RoundToInt(Mathf.Abs((oldCenterChunk - newCenterChunk).y)/chunkSize);



            List<Block> blocksToUpdate = new List<Block>();
            List<Block> blocksToKeep = new List<Block>();

            for (int i = -terrainHalfsize; i < terrainHalfsize + 1; i++)
            {
                for (int j = -terrainHalfsize; j < terrainHalfsize + 1; j++)
                {
                    Vector3 diffPosition = newCenterChunk + new Vector3(i * blockSize, 0, j * blockSize) - oldCenterChunk;

                    (int x, int y) diffPos = ChunkPosition(diffPosition);

                    if (Mathf.Abs(diffPos.x) > terrainHalfsize || Mathf.Abs(diffPos.y) > terrainHalfsize)
                    {
                        newPositions.Enqueue(newCenterChunk + new Vector3(i * blockSize, 0, j * blockSize));
                    }
                }
            }

            (int x, int y) newPos = ChunkPosition(newCenterChunk);

            foreach (Block b in blocks)
            {
                (int x, int y) blockPos = ChunkPosition(b.targetPosition);

                (int x, int y) diffPos = ChunkPosition(new Vector2(Mathf.Abs(b.targetPosition.x - newCenterChunk.x), Mathf.Abs(b.targetPosition.z - newCenterChunk.z)));

                if (diffPos.x > terrainHalfsize || diffPos.y > terrainHalfsize)
                {
                    b.UpdateBlock(newCenterChunk, newPositions.Dequeue());
                } else
                {
                    b.UpdateBlock(newCenterChunk, b.targetPosition);
                }
            }

            
        }

        private void IncrementalUpdate(Vector3 newCenterChunk, Vector3 oldCenterChunk)
        {
            int terrainHalfsize = this.terrainHalfsize * 2 + 1;
            //int terrainHalfsize = this.terrainHalfsize + 1;

            int cnt = 0;
            if (oldCenterChunk.x < newCenterChunk.x)
            {
                for (int i = 0; i < terrainHalfsize; i++)
                {
                    cnt++;
                    newPositions.Enqueue(newCenterChunk + new Vector3(terrainHalfsize / 2, 0, i - terrainHalfsize / 2) * blockSize);
                }
            }
            if (oldCenterChunk.x > newCenterChunk.x)
            {
                for (int i = 0; i < terrainHalfsize; i++)
                {
                    cnt++;
                    newPositions.Enqueue(newCenterChunk + new Vector3(-terrainHalfsize / 2, 0, i - terrainHalfsize / 2) * blockSize);
                }
            }
            if (oldCenterChunk.z < newCenterChunk.z)
            {
                for (int i = 0; i < terrainHalfsize; i++)
                {
                    cnt++;
                    newPositions.Enqueue(newCenterChunk + new Vector3(i - terrainHalfsize / 2, 0, terrainHalfsize / 2) * blockSize);
                }
            }
            if (oldCenterChunk.z > newCenterChunk.z)
            {
                for (int i = 0; i < terrainHalfsize; i++)
                {
                    cnt++;
                    newPositions.Enqueue(newCenterChunk + new Vector3(i - terrainHalfsize / 2, 0, -terrainHalfsize / 2) * blockSize);
                }
            }

            foreach (Block c in blocks)
            {
                if (c.waitingJobResult)
                    continue;

                float dist = terrainHalfsize / 2 * blockSize;
                Vector3 newpos = c.transform.position;
                Vector3 testCenterChunk = centerBlock;
                if (Mathf.Abs(c.transform.position.x - newCenterChunk.x) > dist ||
                    Mathf.Abs(c.transform.position.z - newCenterChunk.z) > dist)
                {
                    //c.transform.position = newPositions.Dequeue();
                    newpos = newPositions.Dequeue();
                }
                UpdateBlock(c, newCenterChunk, newpos);
            }

            centerBlock = newCenterChunk;
        }

        private (int x, int y) ChunkPosition(Vector2 pos)
        {
            int x_ = Mathf.RoundToInt(pos.x / blockSize);
            int y_ = Mathf.RoundToInt(pos.y / blockSize);

            return (x_, y_);
        }

        private (int x, int y) ChunkPosition(Vector3 pos)
        {
            int x_ = Mathf.RoundToInt(pos.x / blockSize);
            int y_ = Mathf.RoundToInt(pos.z / blockSize);

            return (x_, y_);
        }

        private Vector2 ChunkPosition((int x, int y) pos)
        {
            return new Vector2(pos.x * blockSize, pos.y * blockSize);
        }

        public Vector3 GetCenterChunkPosition()
        {
            return transform.position - new Vector3(transform.position.x % blockSize, 0, transform.position.z % blockSize);
        }

        //public (int LOD, int index) GetTerrainLOD(float distance)
        //{
        //    if (meshLOD == null || meshLOD.Length == 0)
        //    {
        //        return (1, 0);
        //    }

        //    for (int i = meshLOD.Length - 1; i >= 0; i--)
        //    {
        //        if (distance >= meshLOD[i].distance)
        //            return (meshLOD[i].LOD, i);
        //    }

        //    return (1, 0);
        //}
    }
}