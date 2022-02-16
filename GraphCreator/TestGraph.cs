using JStuff.GraphCreator;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TestGraph")]
public class TestGraph : Graph
{
    public override Type RootNodeType => typeof(TestRoot);

    public override List<Type> NodeTypes
    {
        get
        {
            List<Type> types = new List<Type>();
            types.Add(typeof(TestNode));
            types.Add(typeof(PropertyNode));
            return types;
        }
    }

    protected override void SetupProperties()
    {
        AddProperty<string>("joe", "seed", PropertyContext.Unique);
        AddProperty<float>(0, "maxValue", PropertyContext.Unique);
        AddProperty<float>(30, "minValue", PropertyContext.Unique);
        AddProperty<int>(30, "current", PropertyContext.Unique);
        AddProperty<int>(30, "hello", PropertyContext.Shared);
    }

    public int Evaluate()
    {
        return ((TestRoot)rootNode).Evaluate();
    }
}