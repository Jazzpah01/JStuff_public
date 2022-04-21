using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.GraphCreator;
using JStuff.Generation;
using JStuff.Dialogue;

namespace JStuff.Generation.Terrain
{
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
            terrainObjects = AddInputLink<List<TerrainObject>>(portName: "TerrainObjects");
        }

        public BlockData Evaluate()
        {
            iteration++;
            BlockData blockData = new BlockData();
            if (meshRendererData.linkedPort != null)
                blockData.meshRendererData = meshRendererData.Evaluate();
            if (meshColliderData.linkedPort != null)
                blockData.meshColliderData = meshColliderData.Evaluate();
            if (colormap.linkedPort != null)
                blockData.colormap = colormap.Evaluate();
            if (terrainObjects.linkedPort != null)
                blockData.terrainObjects = terrainObjects.Evaluate();
            return blockData;
        }
    }
}