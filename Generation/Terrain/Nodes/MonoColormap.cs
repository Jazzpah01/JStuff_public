using JStuff.GraphCreator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.Generation.Terrain
{
    public class MonoColormap : GenerateColormap
    {
        public Color color;

        InputLink<int> sizeInput;
        OutputLink<Color[]> colormapOutput;

        protected override void SetupPorts()
        {
            sizeInput = AddInputLink<int>();
            colormapOutput = AddOutputLink(Evaluate);
        }

        Color[] Evaluate()
        {
            int size = sizeInput.Evaluate();
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