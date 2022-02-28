using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using System;
using JStuff.GraphCreator;

public class Context : ScriptableObject
{
    // For Run mode
    private Dictionary<string, Link> indexer = new Dictionary<string, Link>();
    [NonSerialized]public bool runmode = false;

    // For Editor mode
    public List<string> propertyNames = new List<string>();
    public List<string> propertyTypes = new List<string>();
    public Graph graph;

    [NonSerialized]public bool isSetup = false;

    public bool Contains(string name)
    {
        if (runmode)
        {
            return indexer.ContainsKey(name);
        } else
        {
            return propertyNames.Contains(name);
        }
    }

    public void Initialize(Graph graph)
    {

    }

    public void Clear()
    {
        runmode = false;
        indexer.Clear();
        propertyNames.Clear();
        propertyTypes.Clear();
    }

    public void AddPropertyLink<T>(T value, string propertyName)
    {
        Direction direction = (graph.InputPortDirection == Direction.Input) ? Direction.Output : Direction.Input;
        //propertyName = $"[{typeof(T).Name}] {propertyName}";
        string propertyNameType = $"[{typeof(T).Name}] {propertyName}";
        if (runmode)
        {
            if (!propertyNames.Contains(propertyName))
            {
                Debug.Log(propertyNames.Count);
                Debug.Log(propertyNames[0]);
                throw new System.Exception($"Property name doesn't exist: {propertyName}");
            }

            if (typeof(T).FullName != propertyTypes[propertyNames.IndexOf(propertyName)])
                throw new System.Exception($"Type of property doesn't match: {typeof(T).FullName}");

            Debug.Log("Adding property link!");

            PropertyPort<T> nodePort = new PropertyPort<T>();
            nodePort.Init(null, 0, graph.Orientation, direction, Port.Capacity.Multi);
            nodePort.cachedValue = value;
            indexer.Add(propertyName, nodePort);
        } else
        {
            propertyNames.Add(propertyName);
            propertyTypes.Add(typeof(T).FullName);
        }
    }

    public Link GetPropertyLink(string propertyName)
    {
        if (!runmode)
            throw new System.Exception("Cannot get a property that isn't initialized!");

        if (!Contains(propertyName))
            throw new System.Exception($"Property name of {propertyName} doesn't exist!");

        return indexer[propertyName];
    }

    public string GetPropertyType(string propertyName)
    {
        return propertyTypes[propertyNames.IndexOf(propertyName)];
    }

    public int Length => propertyNames.Count;
}