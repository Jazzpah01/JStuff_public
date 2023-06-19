using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.GraphCreator;
using JStuff.Generation;

namespace JStuff.Generation.Terrain
{
    public class TerrainRoot : Node
    {
        public int colliderLOD = 2;
        //public bool block

        InputLink<MeshData> meshRendererData;
        InputLink<Color[]> colormap;
        InputLink<List<TerrainObject>> terrainObjects;

        [System.Flags]
        public enum BlockDataType
        {
            All = 0b111,
            None = 0b0,
            RenderMesh = 0b1,
            ColliderMesh = 0b10,
            TerrainObjects = 0b100
        }

        protected override void SetupPorts()
        {
            meshRendererData = AddInputLink<MeshData>();
            colormap = AddInputLink<Color[]>(portName: "Colormap");
            terrainObjects = AddInputLink<List<TerrainObject>>(portName: "TerrainObjects");
        }

        public BlockData Evaluate(BlockDataType toEvaluate = BlockDataType.All)
        {
            iteration++;
            BlockData blockData = new BlockData();
            if (meshRendererData.connectedLink != null && toEvaluate.HasFlag(BlockDataType.RenderMesh))
            {
                blockData.meshRendererData = meshRendererData.Evaluate();
            } else
            {
                throw new System.Exception("ERROR: No mesh renderer data connection.");
            }
                
            if (meshRendererData.connectedLink != null && toEvaluate.HasFlag(BlockDataType.ColliderMesh))
            {
                blockData.meshColliderData = TerrainMeshGeneration.GenerateLODMesh(blockData.meshRendererData, colliderLOD);
            }
                
            if (colormap.connectedLink != null && toEvaluate.HasFlag(BlockDataType.RenderMesh))
                blockData.colormap = colormap.Evaluate();
            if (terrainObjects.connectedLink != null && toEvaluate.HasFlag(BlockDataType.TerrainObjects))
                blockData.terrainObjects = terrainObjects.Evaluate();
            return blockData;
        }

        public override Node Clone()
        {
            TerrainRoot retval = base.Clone() as TerrainRoot;
            retval.colliderLOD = colliderLOD;
            return retval;
        }
    }
}