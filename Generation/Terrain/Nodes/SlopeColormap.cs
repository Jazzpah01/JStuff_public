using JStuff.Generation;
using JStuff.GraphCreator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.Utilities;
using JStuff.Collections;

namespace JStuff.Generation.Terrain
{
    [CreateNodePath("Terrain/Colormap/Slope color")]
    public class SlopeColormap : TerrainNode
    {
        public Gradient flatGradient;
        public Gradient slopeGradient;
        public AnimationCurve interpolateFunction;
        public float maxSlope = 10;

        InputLink<MeshData> inputMeshData;
        InputLink<float> maxHeightInput;
        OutputLink<Color[]> colormapOutput;

        public override bool CacheOutput => true;

        protected override void SetupPorts()
        {
            inputMeshData = AddInputLink<MeshData>();
            colormapOutput = AddOutputLink(Evaluate, portName: "Colormap");
        }

        private Color[] Evaluate()
        {
            MeshData data = inputMeshData.Evaluate();
            float maxHeight = data.heightFactor;

            int size = (int)Mathf.Sqrt(data.vertices.Length);
            int asize = size - 1;
            Color[] colormap = new Color[data.vertices.Length];

            Color baseColor = flatGradient.Evaluate(0);

            for (int z = 0; z < size; z++)
            {
                for (int x = 0; x < size; x++)
                {
                    float currentSlope = 0;

                    if ((x - 1).InRange(0, size - 1))
                    {
                        float s = Mathf.Abs(data.vertices[x + z * size].y - data.vertices[x - 1 + z * size].y);
                        if (s > currentSlope)
                            currentSlope = s;
                    }
                    if ((x + 1).InRange(0, size - 1))
                    {
                        float s = Mathf.Abs(data.vertices[x + z * size].y - data.vertices[x + 1 + z * size].y);
                        if (s > currentSlope)
                            currentSlope = s;
                    }
                    if ((z - 1).InRange(0, size - 1))
                    {
                        float s = Mathf.Abs(data.vertices[x + z * size].y - data.vertices[x + (z - 1) * size].y);
                        if (s > currentSlope)
                            currentSlope = s;
                    }
                    if ((z + 1).InRange(0, size - 1))
                    {
                        float s = Mathf.Abs(data.vertices[x + z * size].y - data.vertices[x + (z + 1) * size].y);
                        if (s > currentSlope)
                            currentSlope = s;
                    }

                    currentSlope /= Mathf.Abs(data.vertices[0].x - data.vertices[1].x);

                    float t = interpolateFunction.Evaluate((currentSlope / maxSlope).Clamp(0.0f, 1.0f));

                    float ht = Mathf.Abs(data.vertices[x + z * size].y + maxHeight) / (2 * maxHeight);

                    Color c = flatGradient.Evaluate(Mathf.Clamp(ht, 0, 1));
                    Color c2 = slopeGradient.Evaluate(Mathf.Clamp(ht, 0, 1));

                    colormap[x + z * size] = Color.Lerp(c, c2, t);
                }
            }

            return colormap;
        }

        public override Node Clone()
        {
            SlopeColormap retval = base.Clone() as SlopeColormap;
            retval.slopeGradient = new Gradient();
            retval.slopeGradient.SetKeys(slopeGradient.colorKeys, slopeGradient.alphaKeys);
            retval.flatGradient = new Gradient();
            retval.flatGradient.SetKeys(flatGradient.colorKeys, flatGradient.alphaKeys);
            retval.interpolateFunction = new AnimationCurve(interpolateFunction.keys);
            retval.maxSlope = maxSlope;
            return retval;
        }
    }
}