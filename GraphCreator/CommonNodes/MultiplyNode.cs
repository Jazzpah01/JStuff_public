using JStuff.GraphCreator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiplyNode : CommonNode
{
    InputLink<float> v1;
    InputLink<float> v2;
    OutputLink<float> output;

    protected override void SetupPorts()
    {
        v1 = AddInputLink<float>();
        v2 = AddInputLink<float>();
        output = AddOutputLink<float>(Evaluate);
    }

    public float Evaluate()
    {
        return v1.Evaluate() * v2.Evaluate();
    }
}