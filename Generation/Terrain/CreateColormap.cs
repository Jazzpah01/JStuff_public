using JStuff.GraphCreator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateColormap : Node
{
    public Color color;

    InputLink<int> sizeInput;
    OutputLink<Color[]> colormapOutput;

    protected override void SetupPorts()
    {
        sizeInput = AddInputLink<int>();
        colormapOutput = AddOutputLink<Color[]>(Evaluate);
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
        CreateColormap retval = base.Clone() as CreateColormap;
        retval.color = color;
        return retval;
    }
}