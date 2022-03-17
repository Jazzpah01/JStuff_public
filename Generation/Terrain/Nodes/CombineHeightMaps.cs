using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.GraphCreator;

namespace JStuff.Generation.Terrain
{
    [CreateNodePath("Terrain/HeightMap/Combine")]
    public class CombineHeightMaps : TerrainNode
    {
        [Min(0.00001f)]
        public float scale1 = 1;
        [Min(0.00001f)]
        public float scale2 = 1;

        InputLink<HeightMap> heightMap1Input;
        InputLink<HeightMap> heightMap2Input;
        OutputLink<HeightMap> output;

        protected override void SetupPorts()
        {
            heightMap1Input = AddInputLink<HeightMap>();
            heightMap2Input = AddInputLink<HeightMap>();
            output = AddOutputLink<HeightMap>(Evaluate);
        }

        HeightMap Evaluate()
        {
            HeightMap outer = heightMap1Input.Evaluate();
            HeightMap inner = heightMap2Input.Evaluate();
            float outerScale = scale1;
            float innerScale = scale2;

            if (outer.Length == inner.Length)
            {
                float[,] retval = new float[outer.Length, outer.Length];

                for (int i = 0; i < outer.Length; i++)
                {
                    for (int j = 0; j < outer.Length; j++)
                    {
                        retval[i, j] = outer[i, j] * outerScale + inner[i, j] * innerScale;
                    }
                }

                return new HeightMap(retval);
            } else
            {
                if (outer.Length < inner.Length)
                {
                    HeightMap temp = outer;
                    outer = inner;
                    inner = temp;
                    outerScale = scale2;
                    innerScale = scale1;
                }

                float[,] retval = new float[outer.Length, outer.Length];

                for (int i = 0; i < outer.Length; i++)
                {
                    for (int j = 0; j < outer.Length; j++)
                    {
                        retval[i, j] = outer[i, j] * outerScale + 
                            inner.GetContinousHeight(i / outer.Length * inner.Length, j / outer.Length * inner.Length) * innerScale;
                    }
                }
                return new HeightMap(retval);
            }
        }

        public override Node Clone()
        {
            CombineHeightMaps retval = base.Clone() as CombineHeightMaps;
            retval.scale1 = scale1;
            retval.scale2 = scale2;
            return retval;
        }
    }
}