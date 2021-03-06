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

[Serializable]
public class SimpleNodeView : gvNode, INodeView
{
    public Node node;
    public List<Port> ports = new List<Port>();
    public Map<Port, PortView> portData = new Map<Port, PortView>();
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

        for (int i = 0; i < node.portViews.Count; i++)
        {

        }

        foreach (PortView port in node.portViews)
        {
            Port p = InstantiatePort(port.orientation.Get(), port.direction.Get(), port.capacity.Get(), port.PortType);
            p.portName = port.portName;
            ports.Add(p);
            portData.Add(p, port);

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
        base.SetPosition(newPos);
        node.nodePosition.x = newPos.xMin;
        node.nodePosition.y = newPos.yMin;
    }

    public PortView GetPortView(Port port) => portData.Forward[port];

    public Port GetPort(PortView port) => portData.Reverse[port];

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
        foreach (Port p in ports)
        {
            p.DisconnectAll();
        }
        graphView.PopulateView(graphView.graph);
    }

    public gvNode Node => this;
}