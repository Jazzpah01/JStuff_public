using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.GraphCreator;


namespace JStuff.Generation.Terrain
{
    [CreateNodePath("Terrain/HeightMap/Math/Multiply")]
    public class HeightMapMultiply : TerrainNode
    {
        InputLink<HeightMap> hmInput1;
        InputLink<HeightMap> hmInput2;
        InputLink<float> valueInput;
        OutputLink<HeightMap> output;

        public override bool CacheOutput => true;

        protected override void SetupPorts()
        {
            hmInput1 = AddInputLink<HeightMap>();
            hmInput2 = AddInputLink<HeightMap>("HeightMap (optional)", inputPortSettings: InputPortSettings.Optional);
            valueInput = AddInputLink<float>("Single (optional)", inputPortSettings: InputPortSettings.Optional);
            output = AddOutputLink<HeightMap>(Evaluate);
        }

        public HeightMap Evaluate()
        {
            HeightMap hm = hmInput1.Evaluate();
            float[,] retval = new float[hm.Width, hm.Length];
            float val = (valueInput.connectedLink == null) ? 1 : valueInput.Evaluate();

            if (hmInput2.connectedLink == null)
            {
                for (int y = 0; y < hm.Length; y++)
                {
                    for (int x = 0; x < hm.Width; x++)
                    {
                        retval[x, y] = hm[x, y] * val;
                    }
                }
            }
            else
            {
                HeightMap hm2 = hmInput2.Evaluate();

                for (int y = 0; y < hm.Length; y++)
                {
                    for (int x = 0; x < hm.Width; x++)
                    {
                        retval[x, y] = hm[x, y] * hm2[x, y] * val;
                    }
                }
            }

            return new HeightMap(retval);
        }
    }
}