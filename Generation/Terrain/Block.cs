using JStuff.Generation;
using JStuff.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.Generation.Terrain
{
    public class Block : MonoBehaviour, IConsumer
    {
        [Header("Debug")]
        public bool initialized = false;

        bool inJob = false;

        public WorldTerrain terrain;

        TerrainGraph graph;
        public MeshFilter meshFilter;
        public MeshFilter colliderMeshFilter;
        public MeshCollider meshCollider;
        Material material;

        BlockData oldData;
        BlockData currentData;

        public GameObject colliderObject;

        public Vector3 targetPosition;
        public Vector3 oldTargetPosition;

        public bool waitingJobResult = false;
        public bool waitingCoroutine = false;
        public string coroutine = "";

        public List<GameObject> terrainGameObjects = new List<GameObject>();

        public int iteration = 0;
        int terrainObjectsAmount = 0;

        Mesh[] meshLOD;
        MeshRenderer[] meshRendererLOD;
        int currenMeshLOD = -1;
        int targetLOD = -1;

        public int Priority => priority;
        int priority = 0;

        private void Start()
        {
            meshFilter = GetComponent<MeshFilter>();
        }

        public void Initialize(WorldTerrain worldTerrain, TerrainGraph graph, Material material)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
            gameObject.AddComponent<MeshRenderer>().material = material;
            this.material = material;


            colliderObject = new GameObject("Collider");
            colliderObject.transform.parent = this.transform;
            colliderObject.transform.localPosition = Vector3.zero;
            meshCollider = colliderObject.AddComponent<MeshCollider>();
            colliderObject.SetActive(false);
            colliderObject.layer = gameObject.layer;


            this.graph = graph;
            //this.graph.GetConnections(graph.GetPorts());
            //graph.InitializeGraph();

            terrain = worldTerrain;

            initialized = true;

            oldTargetPosition = new Vector3(0, 0, float.MinValue);

            meshLOD = new Mesh[worldTerrain.meshLOD.Length];
        }

#if UNITY_EDITOR
        public void EditorGenerateRenderMesh()
        {
            BlockData data = ((TerrainGraph)graph).EvaluateGraph(TerrainRoot.BlockDataType.RenderMesh);
            ConsumeJob(data);
        }

        public void EditorGenerateAll()
        {
            BlockData data = ((TerrainGraph)graph).EvaluateGraph();
            ConsumeJob(data);
        }
#endif

        public object Job(object graph)
        {
            BlockData retval = ((TerrainGraph)graph).EvaluateGraph();

            retval._meshRendererData = TerrainMeshGeneration.GenerateLODMeshData(retval.meshRendererData, targetLOD);
            retval._colormap = TerrainMeshGeneration.GenerateLODColormap(retval.colormap, targetLOD);

            retval._meshColliderData = TerrainMeshGeneration.GenerateLODMeshData(retval.meshColliderData, terrain.colliderLOD);

            return retval;
        }

        public void ConsumeJob(object data)
        {
            currentData = (BlockData)data;
            ApplyData();
            waitingJobResult = false;
        }

        public void JobFailed()
        {
            throw new System.NotImplementedException("Job failed!");
        }

        public void UpdateValues(Vector3 centerPosition, Vector3 newPosition)
        {
            graph.CenterPosition = new Vector2(centerPosition.x, centerPosition.z);
            graph.ChunkPosition = new Vector2(newPosition.x, newPosition.z);
            targetPosition = newPosition;
        }

        public void UpdateBlock(Vector3 centerPosition, Vector3 newPosition)
        {
            (int LOD, int index) = terrain.GetTerrainLOD(Vector3.Distance(newPosition, centerPosition));

            if (iteration != 0 && (LOD == currenMeshLOD && newPosition == oldTargetPosition))
                return;

            if (waitingJobResult)
            {
                JobManagerComponent.instance.manager.FinishJobs();
            }

            graph.LOD = LOD;
            graph.CenterPosition = new Vector2(centerPosition.x, centerPosition.z);
            graph.ChunkPosition = new Vector2(newPosition.x, newPosition.z);
            targetPosition = newPosition;
            waitingJobResult = true;
            priority = index;
            targetLOD = LOD;

            if (terrain.enableThreading)
            {
                JobManagerComponent.instance.manager.AddJob(this, Job, graph);
            }
            else
            {
                currentData = Job(graph) as BlockData;
                ConsumeJob(currentData);
            }
        }

        public void ApplyData()
        {
            if (waitingCoroutine)
                StopCoroutine("ApplyDataCoroutine");

            ApplyDataCoroutine();
        }

        IEnumerator ApplyDataCoroutine()
        {
            //TODO: move most of this into TerrainPool and just send the job data there - less copying of data

            waitingCoroutine = true;
            bool newPos = false;

            if (targetPosition != oldTargetPosition)
            {
                iteration++;
                newPos = true;
            }

            // Remove terrain objects
            if (newPos)
            {
                for (int i = 0; i < terrainGameObjects.Count; i++)
                {
                    TerrainPool.QueueDestroyTerrainObject(terrainGameObjects[i], this, iteration);
                    //TerrainPool.DestroyTerrainObject(terrainGameObjects[i]);
                    //terrainGameObjects.RemoveAt(i);
                    //i--;
                }
                //terrainGameObjects.Clear();
            } else if (currentData.terrainObjects != null && currentData.terrainObjects.Count != terrainObjectsAmount)
            {
                int size = terrainGameObjects.Count;
                int j = (currentData.terrainObjects.Count > 0) ? currentData.terrainObjects.Count : 0;
                for (int i = j; i < terrainGameObjects.Count; i++)
                {
                    TerrainPool.QueueDestroyTerrainObject(terrainGameObjects[i], this, iteration);
                    //TerrainPool.DestroyTerrainObject(terrainGameObjects[i]);
                    //terrainGameObjects.RemoveAt(i);
                    //i--;
                }
            }

            // Mesh renderer
            (int LOD, int index) = terrain.GetTerrainLOD(Vector3.Distance(terrain.GetCenterChunkPosition(), targetPosition));

            if (newPos)
            {
                currenMeshLOD = -1;

                SetPosition(targetPosition);
            }

            if (LOD != currenMeshLOD)
            {
                //meshFilter.sharedMesh = TerrainMeshGeneration.GenerateMesh(currentData.meshLOD[index], currentData.colormapLOD[index]);
                TerrainPool.QueueRenderMesh(this, iteration, currentData._meshRendererData, currentData._colormap, targetPosition);
                currenMeshLOD = LOD;
            }

            // Mesh collider
            if (oldData == null || currentData.meshColliderData != oldData.meshColliderData)
            {
                if (currentData.meshColliderData != null && Vector3.Distance(terrain.GetCenterChunkPosition(), targetPosition) < terrain.colliderAtDistance)
                {
                    TerrainPool.QueueColliderMesh(
                        this, iteration, currentData.meshColliderData);
                }
                else
                {
                    colliderObject.SetActive(false);
                }
            }

            // Spawn terrain objects
            if (currentData.terrainObjects != null)
            {
                if (currentData.terrainObjects.Count > terrainObjectsAmount)
                {
                    int j = (currentData.terrainObjects.Count > 0) ? terrainObjectsAmount : 0;
                    for (int i = j; i < currentData.terrainObjects.Count; i++)
                    {
                        TerrainPool.QueueTerrainObject(currentData.terrainObjects[i], this, iteration);
                        //terrainGameObjects.Add(TerrainPool.CreateTerrainObject(currentData.terrainObjects[i], this));
                    }

                    terrainObjectsAmount = currentData.terrainObjects.Count;
                }
            }

            oldTargetPosition = targetPosition;
            oldData = currentData;

            waitingCoroutine = false;

            return null;
        }

        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        public TerrainCoordinate GetCoordinates()
        {
            return new TerrainCoordinate(terrain.blockSize, targetPosition);
        }

        //public float HeightAtPoint(float x, float z)
        //{
        //    int ax = Mathf.FloorToInt(((x - currentPosition.x) / 
        //        mother.chunkSize * currentHeightMap.Width));
        //    int az = Mathf.FloorToInt(((z - currentPosition.z) /
        //        mother.chunkSize * currentHeightMap.Width));

        //    float h = 0;
        //    int i = 0;

        //    try
        //    {
        //        h = currentHeightMap[ax, az];
        //    }
        //    catch
        //    {
        //        throw new System.Exception("Cannot get height from point. x: " + ax + ". z: " + az);
        //    }

        //    return h * mother.meshHeightMultiplier * mother.heightmapMultiplier;
        //}
    }
}