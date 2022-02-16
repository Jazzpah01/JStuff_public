using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.GraphCreator;
using JStuff.Generation;

public class CreateMesh : Node
{
    InputLink<HeightMap> hmInput;
    InputLink<float> meshSizeInput;
    OutputLink<MeshData> output;

    protected override void SetupPorts()
    {
        hmInput = AddInputLink<HeightMap>();
        meshSizeInput = AddInputLink<float>();
        output = AddOutputLink<MeshData>(Evaluate);
    }

    private MeshData Evaluate()
    {
        MeshData data = TerrainMeshGeneration.GenerateMesh(hmInput.Evaluate(), meshSizeInput.Evaluate());
        return data;
    }
}