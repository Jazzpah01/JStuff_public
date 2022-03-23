using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.GraphCreator;

namespace JStuff.Generation.Terrain
{
    [CreateNodePath("Terrain/Mesh/LOD Mesh")]
    public class LODMesh : TerrainNode
    {
        InputLink<int> LODInput;
        InputLink<MeshData> meshDataInput;

        OutputLink<MeshData> output;

        public override bool CacheOutput => true;

        protected override void SetupPorts()
        {
            LODInput = AddInputLink<int>();
            meshDataInput = AddInputLink<MeshData>();

            output = AddOutputLink<MeshData>(Evaluate);
        }

        MeshData Evaluate()
        {
            int LOD = LODInput.Evaluate();
            MeshData meshData = meshDataInput.Evaluate();
            int inputWidth = meshData.Width();

            return TerrainMeshGeneration.GenerateLODMesh(meshData, LOD);
        }
    }
}