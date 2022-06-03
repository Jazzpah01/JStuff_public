using JStuff.Generation;
using JStuff.GraphCreator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.Generation.Terrain
{
    [CreateNodePath("Terrain/Colormap/Gray Scale")]
    public class GrayScale : TerrainNode
    {
        InputLink<MeshData> inputMeshData;
        OutputLink<Color[]> colormapOutput;
        InputLink<float> maxHeightInput;

        protected override void SetupPorts()
        {
            inputMeshData = AddInputLink<MeshData>();
            colormapOutput = AddOutputLink<Color[]>(Evaluate, portName: "Colormap");
        }

        private Color[] Evaluate()
        {
            MeshData data = inputMeshData.Evaluate();
            float maxHeight = data.heightFactor;

            int size = (int)Mathf.Sqrt(data.vertices.Length);
            int asize = size - 1;
            Color[] colormap = new Color[data.vertices.Length];

            for (int z = 0; z < size; z++)
            {
                for (int x = 0; x < size; x++)
                {
                    float h = (data.vertices[x + z * size].y + maxHeight) / (2 * maxHeight);
                    colormap[x + z * size] = Color.Lerp(Color.black, Color.white, h);
                }
            }

            return colormap;
        }
    }
}