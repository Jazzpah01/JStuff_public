using JStuff.GraphCreator;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Port = JStuff.GraphCreator.Port;

namespace JStuff.GraphCreator
{
    public interface INodeView
    {
        UnityEditor.Experimental.GraphView.Port GetPort(Port port);
        UnityEditor.Experimental.GraphView.Node Node { get; }
    }
}

