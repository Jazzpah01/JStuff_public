using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
using JStuff.Collections;
using System;
using gvNode = UnityEditor.Experimental.GraphView.Node;
using gvDirection = UnityEditor.Experimental.GraphView.Direction;
using JStuff.GraphCreator;
using Node = JStuff.GraphCreator.Node;
using JStuff.GraphCreator.Editor;
using UnityEditor;

[Serializable]
public class SimpleNodeView : gvNode, INodeView
{
    public Node node;
    public List<UnityEditor.Experimental.GraphView.Port> ports = new List<UnityEditor.Experimental.GraphView.Port>();
    public Map<UnityEditor.Experimental.GraphView.Port, int> portData = new Map<UnityEditor.Experimental.GraphView.Port, int>();
    public SimpleGraphView graphView;

    public Action<SimpleNodeView> OnNodeSelected;

    public SimpleNodeView(Node node, SimpleGraphView parent)
    {
        this.node = node;
        title = node.name;
        viewDataKey = node.guid;

        style.left = node.nodePosition.x;
        style.top = node.nodePosition.y;

        if (node.StyleSheet != null)
            styleSheets.Add(node.StyleSheet);

        graphView = parent;

        //if (node.portViews == null || node.portViews.Count == 0)
        //{
        //    throw new System.Exception("All nodes must have atleast one port.");
        //}

        for (int i = 0; i < node.ports.Count; i++)
        {

        }

        foreach (JStuff.GraphCreator.Port port in node.ports)
        {
            UnityEditor.Experimental.GraphView.Port p = base.InstantiatePort(port.orientation.Get(), port.direction.Get(), port.capacity.Get(), port.PortType);
            p.portName = port.portName;
            ports.Add(p);

            portData.Add(p, port.nodeIndex);

            if (p.direction.Get() == Link.Direction.Input)
            {
                inputContainer.Add(p);
            }
            else
            {
                outputContainer.Add(p);
            }
        }

        node.OnGUIStart(/*this*/);
        node.OnNodeChange = null;
        node.OnNodeChange = OnNodeChange;
    }

    public override void SetPosition(Rect newPos)
    {
        Undo.RecordObject(node, "Graph Creator (Node position)");
        EditorUtility.SetDirty(node);
        base.SetPosition(newPos);
        node.nodePosition.x = newPos.xMin;
        node.nodePosition.y = newPos.yMin;
    }

    public JStuff.GraphCreator.Port GetPortView(UnityEditor.Experimental.GraphView.Port port) => node.ports[portData.Forward[port]];

    public UnityEditor.Experimental.GraphView.Port GetPort(JStuff.GraphCreator.Port port)
    {
        if (!node.ports.Contains(port))
        {
            throw new Exception($"{port} of {node} doesn't exist in node of nodeview.");
        }

        return portData.Reverse[node.ports.IndexOf(port)];
    }

    public override void OnSelected()
    {
        base.OnSelected();
        if (OnNodeSelected != null)
        {
            OnNodeSelected.Invoke(this);
        }
        node.OnGUISelected(/*this*/);
    }

    public override void OnUnselected()
    {
        base.OnUnselected();
        //if (OnNodeSelected != null)
        //{
        //    OnNodeSelected.Invoke(null);
        //}
        node.OnGUIUnselected(/*this*/);
    }

    private void OnNodeChange()
    {
        foreach (UnityEditor.Experimental.GraphView.Port p in ports)
        {
            p.DisconnectAll();
        }
        graphView.PopulateView(graphView.graph);
    }

    public gvNode Node => this;
}