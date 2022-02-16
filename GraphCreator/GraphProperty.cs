using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GraphProperty
{
    public string propertyName;
    public object value;

    public GraphProperty(string propertyName)
    {
        this.propertyName = propertyName;
    }

    public object GetValue()
    {
        return value;
    }
}