using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.GraphCreator;

namespace JStuff.Generation.Terrain
{
    [CreateNodePath("Terrain/HeightMap/New/Flat")]
    public class FlatNode : TerrainNode
    {
        public int size = 65;
        public float height = 0;
        OutputLink<HeightMap> output;

        public override bool IsConstant()
        {
            return false;
        }

        protected override void SetupPorts()
        {
            output = AddOutputLink<HeightMap>(Evaluate);
        }


        HeightMap Evaluate()
        {
            float[,] hs = new float[size, size];

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    hs[i, j] = height;
                }
            }

            return new HeightMap(hs);
        }

        public override Node Clone()
        {
            FlatNode retval = base.Clone() as FlatNode;

            retval.size = size;
            retval.height = height;

            return retval;
        }
    }
}