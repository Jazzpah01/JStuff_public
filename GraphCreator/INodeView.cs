using JStuff.GraphCreator;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public interface INodeView
{
    Port GetPort(PortView port);
    UnityEditor.Experimental.GraphView.Node Node { get; }
}