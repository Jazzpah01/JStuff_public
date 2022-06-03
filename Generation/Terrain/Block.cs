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

        WorldTerrain terrain;

        TerrainGraph graph;
        MeshFilter meshFilter;
        MeshFilter colliderMeshFilter;
        MeshCollider meshCollider;
        Material material;

        BlockData oldData;
        BlockData currentData;

        GameObject colliderObject;

        public Vector3 targetPosition;
        public Vector3 oldTargetPosition;

        public bool waitingJobResult = false;

        public List<GameObject> terrainGameObjects = new List<GameObject>();
        List<TerrainObject> terrainObjects;

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


            this.graph = graph;
            graph.InitializeGraph();

            terrain = worldTerrain;

            initialized = true;

            oldTargetPosition = new Vector3(0, 0, float.MinValue);
        }

        public object Job(object graph)
        {
            return ((TerrainGraph)graph).EvaluateGraph();
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

        public void UpdateBlock(Vector3 centerPosition, Vector3 newPosition)
        {
            if (waitingJobResult)
            {
                JobManagerComponent.instance.manager.FinishJobs();
            }

            graph.CenterPosition = new Vector2(centerPosition.x, centerPosition.z);
            graph.ChunkPosition = new Vector2(newPosition.x, newPosition.z);
            targetPosition = newPosition;
            waitingJobResult = true;

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
            StartCoroutine(ApplyDataCoroutine());
        }

        IEnumerator ApplyDataCoroutine()
        {
            // Remove terrain objects
            if (terrainObjects != null && currentData.terrainObjects.Count != terrainObjects.Count)
            {
                int size = terrainGameObjects.Count;
                int j = (currentData.terrainObjects.Count > 0) ? currentData.terrainObjects.Count : 0;
                for (int i = j; i < terrainGameObjects.Count; i++)
                {
                    TerrainPool.DestroyTerrainObject(terrainGameObjects[i]);
                    terrainGameObjects.RemoveAt(i);
                    i--;
                }
                terrainGameObjects.Clear();
            }
            yield return null;

            // Mesh renderer
            if (targetPosition != oldTargetPosition)
            {
                Mesh mesh = new Mesh();
                mesh.vertices = currentData.meshRendererData.vertices;
                mesh.uv = currentData.meshRendererData.uv;
                mesh.triangles = currentData.meshRendererData.triangles;
                mesh.colors = currentData.colormap;
                mesh.RecalculateNormals();
                mesh.RecalculateTangents();

                meshFilter.sharedMesh = mesh;

                SetPosition(targetPosition);
            }

            yield return null;

            // Mesh collider
            if (oldData == null || currentData.meshColliderData != oldData.meshColliderData)
            {
                if (currentData.meshColliderData != null)
                {
                    colliderObject.SetActive(true);

                    Mesh colliderMesh = new Mesh();
                    colliderMesh.vertices = currentData.meshColliderData.vertices;
                    colliderMesh.uv = currentData.meshColliderData.uv;
                    colliderMesh.triangles = currentData.meshColliderData.triangles;

                    meshCollider.sharedMesh = colliderMesh;
                }
                else
                {
                    colliderObject.SetActive(false);
                }
            }

            yield return null;

            // Spawn terrain objects
            if (currentData.terrainObjects != null)
            {
                if (terrainObjects == null)
                    terrainObjects = new List<TerrainObject>();

                if (currentData.terrainObjects.Count > terrainObjects.Count)
                {
                    int j = (currentData.terrainObjects.Count > 0) ? terrainObjects.Count : 0;
                    for (int i = j; i < currentData.terrainObjects.Count; i++)
                    {
                        terrainGameObjects.Add(TerrainPool.CreateTerrainObject(currentData.terrainObjects[i], this));
                    }

                    terrainObjects = currentData.terrainObjects;
                }
            }

            yield return null;

            oldTargetPosition = targetPosition;
            oldData = currentData;
        }

        public void SetPosition(Vector3 position)
        {
            transform.position = position;
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