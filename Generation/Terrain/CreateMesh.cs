using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.GraphCreator;
using JStuff.Generation;

public class CreateMesh : Node
{
    InputLink<HeightMap> hmInput;
    InputLink<float> meshSizeInput;
    InputLink<float> scaleInput;
    OutputLink<MeshData> output;
    OutputLink<int> sizeOutput;

    public override bool CacheOutput => true;

    protected override void SetupPorts()
    {
        hmInput = AddInputLink<HeightMap>();
        meshSizeInput = AddInputLink<float>();
        scaleInput = AddInputLink<float>();
        output = AddOutputLink<MeshData>(Evaluate);
        sizeOutput = AddOutputLink<int>(EveluateSize);
    }

    private MeshData Evaluate()
    {
        MeshData data = TerrainMeshGeneration.GenerateMesh(hmInput.Evaluate(), meshSizeInput.Evaluate(), scaleInput.Evaluate());
        return data;
    }

    private int EveluateSize()
    {
        MeshData data = TerrainMeshGeneration.GenerateMesh(hmInput.Evaluate(), meshSizeInput.Evaluate(), scaleInput.Evaluate());
        return data.vertices.Length;
    }
}