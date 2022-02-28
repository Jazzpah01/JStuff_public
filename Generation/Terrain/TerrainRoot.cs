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
    InputLink<List<TerrainObject>> gameObjects;

    protected override void SetupPorts()
    {
        meshRendererData = AddInputLink<MeshData>();
        meshColliderData = AddInputLink<MeshData>();
        colormap = AddInputLink<Color[]>();
        gameObjects = AddInputLink<List<TerrainObject>>();
    }

    public BlockData Evaluate()
    {
        BlockData blockData = new BlockData();
        blockData.meshRendererData = meshRendererData.Evaluate();
        blockData.meshColliderData = meshColliderData.Evaluate();
        blockData.colormap = colormap.Evaluate();
        blockData.terrainObjects = gameObjects.Evaluate();
        return blockData;
    }
}