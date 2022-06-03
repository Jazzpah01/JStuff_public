using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.GraphCreator;
using JStuff.Generation;

namespace JStuff.Generation.Terrain
{
    [CreateNodePath("Terrain/Mesh/Offset")]
    public class OffsetMesh : TerrainNode
    {
        InputLink<MeshData> meshInput;
        InputLink<float> offsetInput;

        OutputLink<MeshData> output;

        public override bool CacheOutput => true;

        protected override void SetupPorts()
        {
            meshInput = AddInputLink<MeshData>();
            offsetInput = AddInputLink<float>();

            output = AddOutputLink(Evaluate);
        }

        private MeshData Evaluate()
        {
            MeshData data = meshInput.Evaluate().Clone();

            float offset = offsetInput.Evaluate();

            for (int i = 0; i < data.sizeX; i++)
            {
                for (int j = 0; j < data.sizeZ; j++)
                {
                    data.vertices[i + j * data.sizeX].y += offset;
                }
            }

            return data;
        }
    }
}