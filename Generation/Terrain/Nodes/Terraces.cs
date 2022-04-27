using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.GraphCreator;

namespace JStuff.Generation.Terrain
{
    [CreateNodePath("Terrain/HeightMap/Terraces")]
    public class Terraces : TerrainNode
    {
        public int levels = 9;

        InputLink<HeightMap> input;
        OutputLink<HeightMap> output;

        protected override void SetupPorts()
        {
            input = AddInputLink<HeightMap>();
            output = AddOutputLink<HeightMap>(Evaluate);
        }

        HeightMap Evaluate()
        {
            float[,] retval = input.Evaluate().ToArray();

            for (int y = 0; y < retval.GetLength(1); y++)
            {
                for (int x = 0; x < retval.GetLength(0); x++)
                {
                    retval[x, y] = Mathf.Round((retval[x, y] + 1) / 2 * levels) / levels * 2 - 1;
                }
            }

            return new HeightMap(retval);
        }

        public override Node Clone()
        {
            Terraces retval = base.Clone() as Terraces;
            retval.levels = levels;
            return retval;
        }
    }
}