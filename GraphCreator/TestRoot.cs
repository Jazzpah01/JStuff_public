using JStuff.GraphCreator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestRoot : Node
{
    InputLink<int> link;

    protected override void SetupPorts()
    {
         link = AddInputLink<int>();
    }

    public int Evaluate()
    {
        return link.Evaluate();
    }
}