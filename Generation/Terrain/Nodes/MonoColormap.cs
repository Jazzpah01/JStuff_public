using JStuff.GraphCreator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.Generation.Terrain
{
    public class MonoColormap : GenerateColormap
    {
        public Color color;

        InputLink<MeshData> meshData;
        OutputLink<Color[]> colormapOutput;

        protected override void SetupPorts()
        {
            meshData = AddInputLink<MeshData>();
            colormapOutput = AddOutputLink(Evaluate);
        }

        Color[] Evaluate()
        {
            MeshData data = meshData.Evaluate();
            int size = data.vertices.Length;
            Color[] output = new Color[size];

            for (int i = 0; i < size; i++)
            {
                output[i] = color;
            }

            return output;
        }

        public override Node Clone()
        {
            MonoColormap retval = base.Clone() as MonoColormap;
            retval.color = color;
            return retval;
        }
    }
}