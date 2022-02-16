using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public TerrainGraph graph;
    MeshFilter meshFilter;

    private void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        Initialize();
    }

    public void Initialize()
    {
        graph.SetupGraph();

        BlockData data = graph.EvaluateGraph();

        Mesh mesh = new Mesh();
        mesh.vertices = data.meshRendererData.vertices;
        mesh.uv = data.meshRendererData.uv;
        mesh.triangles = data.meshRendererData.triangles;
        mesh.colors = data.colormap;
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }
}