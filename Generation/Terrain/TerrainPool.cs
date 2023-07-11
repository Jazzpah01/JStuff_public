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
            //public int LOD;

            public RenderMeshDataJob(Block block, int blockIteration, MeshData meshData, Color[] colormap, Vector3 targetPosition)
            {
                this.block = block;
                this.blockIteration = blockIteration;
                this.meshData = meshData;
                this.colormap = colormap;
                this.targetPosition = targetPosition;
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

        public static void QueueRenderMesh(Block block, int iteration, MeshData meshData, Color[] colormap, Vector3 targetPosition)
        {
            renderMeshJobs.Enqueue(new RenderMeshDataJob(block, iteration, meshData, colormap, targetPosition));
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
                    Mesh mesh = new Mesh();
                    mesh.vertices = job.meshData.vertices;
                    mesh.uv = job.meshData.uv;
                    mesh.triangles = job.meshData.triangles;
                    mesh.colors = job.colormap;
                    mesh.RecalculateNormals();
                    mesh.RecalculateTangents();

                    job.block.meshFilter.sharedMesh = mesh;

                    job.block.SetPosition(job.targetPosition);
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