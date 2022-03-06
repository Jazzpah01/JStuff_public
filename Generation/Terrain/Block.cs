using JStuff.Generation;
using JStuff.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.Generation.Terrain
{
    public class Block : MonoBehaviour, IConsumer
    {
        WorldTerrain terrain;

        TerrainGraph graph;
        MeshFilter meshFilter;
        Material material;

        BlockData currentData;

        Vector3 targetPosition;

        public bool waitingJobResult = false;

        private void Start()
        {
            meshFilter = GetComponent<MeshFilter>();
        }

        public void Initialize(WorldTerrain worldTerrain, TerrainGraph graph, Material material)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
            gameObject.AddComponent<MeshRenderer>().material = material;
            this.material = material;

            this.graph = graph;
            graph.SetupGraph();
            graph.ChunkSize = worldTerrain.chunkSize;
            graph.Scale = worldTerrain.scale;
            graph.Seed = worldTerrain.seed;
            graph.Zoom = worldTerrain.zoom;

            terrain = worldTerrain;
        }

        public object Job(object graph)
        {
            return ((TerrainGraph)graph).EvaluateGraph();
        }

        public void ConsumeJob(object data)
        {
            currentData = (BlockData)data;
            ApplyData();
            SetPosition(targetPosition);
            waitingJobResult = false;
        }

        public void JobFailed()
        {
            throw new System.NotImplementedException("Job failed!");
        }

        public void UpdateBlock(Vector3 centerPosition, Vector3 newPosition)
        {
            Debug.Log("Position: " + newPosition);
            graph.CenterPosition = new Vector2(centerPosition.x, centerPosition.z);
            graph.ChunkPosition = new Vector2(newPosition.x, newPosition.z);
            Debug.Log("Calculating!");
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
            Mesh mesh = new Mesh();
            mesh.vertices = currentData.meshRendererData.vertices;
            mesh.uv = currentData.meshRendererData.uv;
            mesh.triangles = currentData.meshRendererData.triangles;
            mesh.colors = currentData.colormap;
            mesh.RecalculateNormals();

            meshFilter.sharedMesh = mesh;

            Debug.Log(currentData.meshRendererData.vertices.Length);
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