using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.GraphCreator;
using JStuff.Generation;
using JStuff.Dialogue;

namespace JStuff.Generation.Terrain
{
    [CreateNodePath("Terrain/HeightMap/Function")]
    public class HeightMapFunction : TerrainNode
    {
        public float scale = 1;
        public AnimationCurve function;

        InputLink<HeightMap> input;

        OutputLink<HeightMap> output;

        public override bool CacheOutput => true;

        protected override void SetupPorts()
        {
            input = AddInputLink<HeightMap>();
            output = AddOutputLink(GenerateHeightMap);
        }

        public HeightMap GenerateHeightMap()
        {
            float[,] array = input.Evaluate().Array;
            float[,] retval = new float[array.GetLength(0), array.GetLength(1)];

            for (int y = 0; y < array.GetLength(1); y++)
            {
                for (int x = 0; x < array.GetLength(0); x++)
                {
                    retval[x, y] = Mathf.Clamp01(function.Evaluate((array[x, y] + 1) / 2)) * 2 - 1;
                }
            }

            return new HeightMap(retval);
        }

        public override Node Clone()
        {
            HeightMapFunction retval = base.Clone() as HeightMapFunction;
            retval.scale = scale;
            retval.function = new AnimationCurve(function.keys);
            return retval;
        }
    }
}