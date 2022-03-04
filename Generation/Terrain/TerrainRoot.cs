using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.GraphCreator;
using JStuff.Generation;

public class TerrainRoot : Node
{
    InputLink<MeshData> meshRendererData;
    InputLink<MeshData> meshColliderData;
    InputLink<Color[]> colormap;
    InputLink<List<TerrainObject>> terrainObjects;

    protected override void SetupPorts()
    {
        meshRendererData = AddInputLink<MeshData>();
        meshColliderData = AddInputLink<MeshData>();
        colormap = AddInputLink<Color[]>(portName: "Colormap");
        terrainObjects = AddInputLink<List<TerrainObject>>(portName: "List<TerrainObject>");
    }

    public BlockData Evaluate()
    {
        iteration++;
        BlockData blockData = new BlockData();
        if (meshRendererData != null)
            blockData.meshRendererData = meshRendererData.Evaluate();
        if (meshColliderData != null)
            blockData.meshColliderData = meshColliderData.Evaluate();
        if (colormap != null)
            blockData.colormap = colormap.Evaluate();
        if (terrainObjects != null)
            blockData.terrainObjects = terrainObjects.Evaluate();
        return blockData;
    }
}