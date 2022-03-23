using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.GraphCreator;
using JStuff.Generation;

namespace JStuff.Generation.Terrain
{
    [CreateNodePath("Terrain/Mesh/Standard")]
    public class StandardMesh : TerrainNode
    {
        InputLink<HeightMap> hmInput;
        InputLink<float> scaleInput;

        InputLink<float> chunkSizeInput;

        OutputLink<MeshData> output;

        public override bool CacheOutput => true;

        protected override void SetupPorts()
        {
            hmInput = AddInputLink<HeightMap>();
            scaleInput = AddInputLink<float>();

            chunkSizeInput = AddPropertyInputLink<float>("chunkSize");

            output = AddOutputLink(Evaluate);
        }

        private MeshData Evaluate()
        {
            MeshData data = TerrainMeshGeneration.GenerateMesh(hmInput.Evaluate(), chunkSizeInput.Evaluate(), scaleInput.Evaluate());
            return data;
        }
    }
}