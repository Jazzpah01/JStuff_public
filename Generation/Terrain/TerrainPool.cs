using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace JStuff.Generation.Terrain
{
    public class TerrainPool : MonoBehaviour
    {
        public struct InstantiationJob
        {
            public TerrainObject terrainObject;
            public Block block;
            public int blockIteration;

            public InstantiationJob(TerrainObject terrainObject, Block block, int blockIteration)
            {
                this.terrainObject = terrainObject;
                this.block = block;
                this.blockIteration = blockIteration;
            }
        }

        public struct DestroyJob
        {
            public GameObject gameObject;
            public Block block;
            public int blockIteration;

            public DestroyJob(GameObject gameObject, Block block, int blockIteration)
            {
                this.gameObject = gameObject;
                this.block = block;
                this.blockIteration = blockIteration;
            }
        }

        public struct RenderMeshDataJob
        {
            public Block block;
            public int blockIteration;
            public MeshData meshData;
            public Color[] colormap;
            public Vector3 targetPosition;
            public bool recalculateSeams;
            public bool recalculateNormals;
            //public int LOD;

            public RenderMeshDataJob(Block block, int blockIteration, MeshData meshData, Color[] colormap, Vector3 targetPosition, bool recalculateSeams, bool recalculateNormals)
            {
                this.block = block;
                this.blockIteration = blockIteration;
                this.meshData = meshData;
                this.colormap = colormap;
                this.targetPosition = targetPosition;
                this.recalculateSeams = recalculateSeams;
                this.recalculateNormals = recalculateNormals;
                //this.LOD = LOD;
            }
        }

        public struct ColliderMeshDataJob
        {
            public Block block;
            public int blockIteration;
            public MeshData meshData;
            //public int LOD;

            public ColliderMeshDataJob(Block block, int blockIteration, MeshData meshData)
            {
                this.block = block;
                this.blockIteration = blockIteration;
                this.meshData = meshData;
                //this.LOD = LOD;
            }
        }

        public float budget_ms = 2f;
        public float max_budged_ms = 4f;
        public float workload = 0f;
        public float elapsed_ms = 0f;

        private int current = 0;
        private int changed = 0;

        private int initializing = 10;

        private static Queue<InstantiationJob> instantiationJobs = new Queue<InstantiationJob>();
        private static Queue<DestroyJob> destroyJobs = new Queue<DestroyJob>();
        private static Queue<RenderMeshDataJob> renderMeshJobs = new Queue<RenderMeshDataJob>();
        private static Queue<ColliderMeshDataJob> colliderMeshJobs = new Queue<ColliderMeshDataJob>();

        private static Dictionary<GameObject, Stack<GameObject>> freeObjects = new Dictionary<GameObject, Stack<GameObject>>();
        private static Dictionary<GameObject, GameObject> usedObjects = new Dictionary<GameObject, GameObject>();

        private static TerrainPool instance;

        private static Stopwatch stopwatch = new Stopwatch();

        static Dictionary<int, Stack<Vector3[]>> unusedSeamNormals = new Dictionary<int, Stack<Vector3[]>>();
        static Dictionary<int, Stack<Color[]>> unusedSeamColormaps = new Dictionary<int, Stack<Color[]>>();

        private void Start()
        {
            // TESTING array seam index
            int[] testArray = new int[] {
                5, 3, 5,
                2, 5, 0,
                5, 1, 5,
            };

            for (int i = 0; i < 4; i++)
            {
                string s = "";
                for (int j = 0; j < 3; j++)
                {
                    int arrayIndex = Seam.SeamToArrayIndex(3, i, j);
                    s += testArray[arrayIndex];
                }
                UnityEngine.Debug.Log($"TESTING SEAM INDEX {i}: {s}");
            }

            // TESTING mesh GetXZ
            float[,] testFArray = new float[,]
            {
                { 0.5f, 0.5f, 0.5f },
                { 0.5f, 0.5f, 0.5f },
                { 0.5f, 0.5f, 0.5f }
            };

            HeightMap hm = new HeightMap(testFArray);

            var meshdata = TerrainMeshGeneration.GenerateMeshData(hm, 10);

            UnityEngine.Debug.Log($"TESTING MESH GetXZ: index {0} has {meshdata.GetXZ(0)}");
            UnityEngine.Debug.Log($"TESTING MESH GetXZ: index {2} has {meshdata.GetXZ(2)}");
            UnityEngine.Debug.Log($"TESTING MESH GetXZ: index {4} has {meshdata.GetXZ(4)}");
            UnityEngine.Debug.Log($"TESTING MESH GetXZ: index {8} has {meshdata.GetXZ(8)}");
        }

        public static Vector3[] GetSeamNormals(int length)
        {
            if (unusedSeamNormals.ContainsKey(length) && unusedSeamNormals[length].Count > 0)
            {
                return unusedSeamNormals[length].Pop();
            } else
            {
                return new Vector3[length];
            }
        }

        public static void RemoveSeamNormals(Vector3[] seamNormals)
        {
            if (seamNormals == null || seamNormals.Length == 0)
                return;

            if (unusedSeamNormals.ContainsKey(seamNormals.Length))
            {
                unusedSeamNormals[seamNormals.Length].Push(seamNormals);
            }
            else
            {
                unusedSeamNormals.Add(seamNormals.Length, new Stack<Vector3[]>());
                unusedSeamNormals[seamNormals.Length].Push(seamNormals);
            }
        }

        public static Color[] GetSeamColormap(int length)
        {
            if (unusedSeamNormals.ContainsKey(length) && unusedSeamNormals[length].Count > 0)
            {
                return unusedSeamColormaps[length].Pop();
            }
            else
            {
                return new Color[length];
            }
        }

        public static void RemoveSeamColormap(Color[] seamColormap)
        {
            if (seamColormap == null || seamColormap.Length == 0)
                return;

            if (unusedSeamColormaps.ContainsKey(seamColormap.Length))
            {
                unusedSeamColormaps[seamColormap.Length].Push(seamColormap);
            }
            else
            {
                unusedSeamColormaps.Add(seamColormap.Length, new Stack<Color[]>());
                unusedSeamColormaps[seamColormap.Length].Push(seamColormap);
            }
        }

        private void Awake()
        {
            freeObjects = new Dictionary<GameObject, Stack<GameObject>>();
            usedObjects = new Dictionary<GameObject, GameObject>();
            instance = this;
        }

        private void OnDestroy()
        {
            freeObjects = new Dictionary<GameObject, Stack<GameObject>>();
            usedObjects = new Dictionary<GameObject, GameObject>();
            instance = this;
        }

        public static void DestroyTerrainObject(GameObject go)
        {
            if (!go.activeSelf)
                return;

            if (!Application.isPlaying)
            {
                Destroy(go);
                return;
            }

            GameObject prefab = usedObjects[go];
            usedObjects.Remove(go);

            if (!freeObjects.ContainsKey(prefab))
                freeObjects.Add(prefab, new Stack<GameObject>());

            freeObjects[prefab].Push(go);

            //go.transform.parent = instance.transform;

            go.SetActive(false);
        }

        public static void QueueTerrainObject(TerrainObject terrainObject, Block block, int iteration)
        {
            instantiationJobs.Enqueue(new InstantiationJob(terrainObject, block, iteration));
        }

        public static void QueueDestroyTerrainObject(GameObject go, Block block, int iteration)
        {
            destroyJobs.Enqueue(new DestroyJob(go, block, iteration));
        }

        public static void QueueRenderMesh(Block block, int iteration, MeshData meshData, Color[] colormap, Vector3 targetPosition, bool recalculateSeams, bool recalculateNormals)
        {
            renderMeshJobs.Enqueue(new RenderMeshDataJob(block, iteration, meshData, colormap, targetPosition, recalculateSeams, recalculateNormals));
        }

        public static void QueueColliderMesh(Block block, int iteration, MeshData meshData)
        {
            colliderMeshJobs.Enqueue(new ColliderMeshDataJob(block, iteration, meshData));
        }

        public static GameObject CreateTerrainObject(TerrainObject terrainObject, Block block)
        {
            if (!Application.isPlaying)
            {
                GameObject go = Instantiate(terrainObject.prefab);
                //go.tag = "Spawned";

                SetGameObjectValues(terrainObject, go, block);
                return go;
            }

            if (!freeObjects.ContainsKey(terrainObject.prefab) || freeObjects[terrainObject.prefab].Count <= 0)
            {
                GameObject go = Instantiate(terrainObject.prefab);
                //go.tag = "Spawned";

                SetGameObjectValues(terrainObject, go, block);

                usedObjects.Add(go, terrainObject.prefab);

                return go;
            } else
            {
                GameObject go = freeObjects[terrainObject.prefab].Pop();
                //go.tag = "Spawned";

                SetGameObjectValues(terrainObject, go, block);

                go.SetActive(true);

                usedObjects.Add(go, terrainObject.prefab);

                return go;
            }
        }

        private void Update()
        {
            stopwatch.Restart();

            float b = Mathf.Min(budget_ms + budget_ms * Mathf.Max(workload - 1, 0), max_budged_ms);

            if (initializing > 0)
                b = 1000f;

            while (stopwatch.Elapsed.TotalMilliseconds < b && destroyJobs.Count > 0)
            {
                DestroyJob job = destroyJobs.Dequeue();

                if (job.block.iteration == job.blockIteration)
                {
                    DestroyTerrainObject(job.gameObject);
                    job.block.terrainGameObjects.Remove(job.gameObject);
                }
            }

            while (stopwatch.Elapsed.TotalMilliseconds < b && renderMeshJobs.Count > 0)
            {
                RenderMeshDataJob job = renderMeshJobs.Dequeue();

                if (job.blockIteration == job.block.iteration)
                {
                    TerrainCoordinate blockCoordinates = job.block.GetCoordinates();
                    TerrainCoordinate[] coordinatesInDirection = new TerrainCoordinate[] { 
                        blockCoordinates + new TerrainCoordinate(1, 0),
                        blockCoordinates + new TerrainCoordinate(0, 1),
                        blockCoordinates + new TerrainCoordinate(-1, 0),
                        blockCoordinates + new TerrainCoordinate(0, -1),
                    };

                    bool seamless = true;
                    bool redoSeams = false;
                    bool hasAllNeighbors = true;

                    if (job.recalculateSeams)
                    {
                        bool hasExtrusions = false;
                        for (int i = 0; i < 4; i++)
                        {
                            if (job.block.seamExtrusion[i])
                            {
                                hasExtrusions = true;
                                job.block.seamExtrusion[i] = false;
                            }
                        }

                        if (hasExtrusions)
                            Seam.ResetExtrusion(job.block.currentData._meshRendererData, job.block.currentData.meshRendererData);

                        // Seam extrusion
                        for (int i = 0; i < 4; i++)
                        {
                            job.block.seamExtrusion[i] = false;
                        }

                        List<int> extrusionDirections = job.block.GetLODEdgeDirections();

                        foreach (var direction in extrusionDirections)
                        {
                            int LOD = job.block.GetNeighborLOD(direction);

                            int LODDiff = LOD / job.block.targetLOD;

                            if (LODDiff <= 0)
                                continue;

                            job.block.seamExtrusion[direction] = true;
                            Seam.ExtrudeEdgeVertices(job.block.currentData._meshRendererData, LODDiff, direction);
                        }
                    }

                    if (job.recalculateNormals)
                    {
                        job.block.currentData._meshRendererData.CalculateUnormalizedNormals(ref job.block.currentData._normals);

                        int LOD = ((int)Mathf.Sqrt(job.block.currentData.colormap.Length) - 1) / (int)(Mathf.Sqrt(job.block.currentData._colormap.Length) - 1);

                        job.block.currentData._colormap = TerrainMeshGeneration.GenerateLODColormap(job.block.currentData.colormap, LOD);

                        for (int i = 0; i < 4; i++)
                        {
                            job.block.seamSize[i] = -1;
                        }
                    }

                    for (int direction = 0; direction < 4; direction++)
                    {
                        if (WorldTerrain.instance.blockOfCoordinates.ContainsKey(coordinatesInDirection[direction]) && 
                            WorldTerrain.instance.blockOfCoordinates[coordinatesInDirection[direction]] &&
                            job.block.seamSize[direction] < 0)
                        {
                            int oppositeDirection = (direction + 2) % 4;

                            Block thisBlock = job.block;
                            Block otherBlock = WorldTerrain.instance.blockOfCoordinates[coordinatesInDirection[direction]];

                            int seamSize = Mathf.RoundToInt(Mathf.Sqrt(thisBlock.currentData._colormap.Length));

                            if (otherBlock.currentData != null)
                            {
                                MeshData thisMesh = thisBlock.currentData._meshRendererData;
                                MeshData otherMesh = otherBlock.currentData.meshRendererData;

                                Color[] thisColormap = thisBlock.currentData._colormap;
                                Vector3[] thisNormals = thisBlock.currentData._normals;

                                Color[] otherColormap = otherBlock.currentData.colormap;

                                Seam.UpdateSeamNormals(otherMesh, ref thisNormals, direction);
                                Seam.UpdateSeamColormap(ref thisColormap, otherColormap, direction);

                                job.block.seamSize[direction] = seamSize;
                            }
                            else
                            {
                                redoSeams = true;
                            }
                        }
                        else
                        {
                            seamless = false;
                            hasAllNeighbors = false;
                            job.block.seamSize[direction] = -1;
                        }
                    }

                    // normalize
                    for (int i = 0; i < job.block.currentData._normals.Length; i++)
                    {
                        job.block.currentData._normals[i] = job.block.currentData._normals[i].normalized;
                    }

                    Mesh mesh = new Mesh();
                    mesh.vertices = job.meshData.vertices;
                    mesh.uv = job.meshData.uv;
                    mesh.triangles = job.meshData.triangles;

                    mesh.normals = job.block.currentData._normals;
                    mesh.colors = job.block.currentData._colormap;

                    job.block.seamless = seamless;

                    job.block.meshFilter.sharedMesh = mesh;

                    job.block.SetPosition(job.targetPosition);

                    if (redoSeams)
                    {
                        QueueRenderMesh(job.block, job.blockIteration, job.meshData, job.colormap, job.targetPosition, true, true);
                    }
                }
            }

            while (stopwatch.Elapsed.TotalMilliseconds < b && colliderMeshJobs.Count > 0)
            {
                ColliderMeshDataJob job = colliderMeshJobs.Dequeue();

                if (job.blockIteration == job.block.iteration)
                {
                    job.block.colliderObject.SetActive(true);

                    Mesh colliderMesh = TerrainMeshGeneration.GenerateMesh(job.meshData, null);

                    job.block.meshCollider.sharedMesh = colliderMesh;
                }
            }

            while (stopwatch.Elapsed.TotalMilliseconds < b && instantiationJobs.Count > 0)
            {
                InstantiationJob job = instantiationJobs.Dequeue();

                if (job.blockIteration == job.block.iteration)
                {
                    GameObject go = CreateTerrainObject(job.terrainObject, job.block);
                    job.block.terrainGameObjects.Add(go);
                }
            }

            changed = renderMeshJobs.Count + colliderMeshJobs.Count + instantiationJobs.Count - current;
            current = renderMeshJobs.Count + colliderMeshJobs.Count + instantiationJobs.Count;

            workload += (float)changed;
            elapsed_ms = (float)stopwatch.Elapsed.TotalMilliseconds;

            if (renderMeshJobs.Count + colliderMeshJobs.Count + instantiationJobs.Count == 0 && initializing > 0)
                initializing--;
        }

        private static void SetGameObjectValues(TerrainObject terrainObject, GameObject go, Block block)
        {
            go.transform.position = terrainObject.position + block.transform.position;
            go.transform.rotation = terrainObject.rotation;
            go.transform.localScale = Vector3.one * terrainObject.scale;
            //go.transform.parent = block.transform;
            go.transform.parent = instance.transform;
        }
    }
}