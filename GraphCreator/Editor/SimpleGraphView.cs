using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System;
using System.Linq;
using JStuff.GraphCreator;
using Node = JStuff.GraphCreator.Node;

public class SimpleGraphView : UnityEditor.Experimental.GraphView.GraphView
{
    public new class UxmlFactory : UxmlFactory<SimpleGraphView, UxmlTraits> { }

    public Graph graph;

    public Action<SimpleNodeView> OnNodeSelected;

    public SimpleGraphView()
    {
        Insert(0, new GridBackground());

        this.AddManipulator(new ContentZoomer());
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/JStuff_public/GraphCreator/Editor/SimpleGraphEditor.uss");
        styleSheets.Add(styleSheet);
    }

    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        if (graph == null)
        {
            Debug.LogWarning("Cannot edit without a graph selected.");
            return;
        }

        evt.menu.AppendAction("ReloadGraph", delegate { graph.ResetPorts(); PopulateView(graph); OnNodeSelected(FindNodeView(graph.rootNode)); });

        foreach (Type type in graph.NodeTypes)
        {
            AddMenuType(evt, type);
        }
    }

    private void AddMenuType(ContextualMenuPopulateEvent evt, Type t)
    {
        var types = TypeCache.GetTypesDerivedFrom(t);
        Vector2 localMousePos = evt.localMousePosition;
        Vector2 actualGraphPosition = viewTransform.matrix.inverse.MultiplyPoint(localMousePos);
        GetNodePath(evt, t, actualGraphPosition);
        //foreach (var type in types)
        //{
        //    evt.menu.AppendAction($"{type.BaseType.Name} / {type.Name}", (a) => CreateNode(type, actualGraphPosition));
        //}
        //if (!t.IsAbstract)
        //{
        //    evt.menu.AppendAction($"[{t.BaseType.Name}] {t.Name}", (a) => CreateNode(t, actualGraphPosition));
        //}
    }

    private void GetNodePath(ContextualMenuPopulateEvent evt, Type type, Vector2 pos, string path = "")
    {
        if (!type.IsAbstract)
        {
            evt.menu.AppendAction($"{path}{type.Name}", (a) => CreateNode(type, pos));
        } else
        {
            var types = TypeCache.GetTypesDerivedFrom(type);
            foreach (var t in types)
            {
                if (t.BaseType == type)
                {
                    GetNodePath(evt, t, pos, $"{path}{type.Name}/");
                }
            }
        }
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        return ports.ToList().Where(
            endPort =>
            startPort.direction != endPort.direction &&
            startPort.portType == endPort.portType).ToList();
    }

    internal void PopulateView(Graph g)
    {
        graph = g;

        graphViewChanged -= OnGraphViewChanged;
        DeleteElements(graphElements);
        graphViewChanged += OnGraphViewChanged;

        // Create node view
        graph.UpdateNodes();
        foreach (Node node in graph.nodes)
        {
            CreateNodeView(node);
        }

        // Create root node
        if (graph.rootNode == null)
        {
            CreateNode(graph.RootNodeType);
        }

        // Set root nodeview properties
        FindNodeView(graph.rootNode).capabilities &= ~Capabilities.Deletable;

        // Create edges
        foreach (Node node in graph.nodes)
        {
            foreach (PortView port in node.portViews)
            {
                if (port.node == null || port.linked.Count < 1)
                    continue;

                Node inputnode = port.node;

                foreach (PortView view in port.linked)
                {
                    Node outputnode = view.node;

                    Edge edge = FindNodeView(inputnode).GetPort(port).ConnectTo(FindNodeView(outputnode).GetPort(view));
                    edge.input.portType = port.PortType;
                    edge.output.portType = port.PortType;
                    AddElement(edge);
                }
            }
        }
    }

    SimpleNodeView FindNodeView(Node node)
    {
        return GetNodeByGuid(node.guid) as SimpleNodeView;
    }

    private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
    {
        if (graphViewChange.elementsToRemove != null)
        {
            foreach (GraphElement element in graphViewChange.elementsToRemove)
            {
                SimpleNodeView view = element as SimpleNodeView;
                if (view != null)
                {
                    graph.DeleteNode(view.node);
                }

                Edge edge = element as Edge;
                if (edge != null)
                {
                    SimpleNodeView inputNode = (SimpleNodeView)edge.input.node;
                    SimpleNodeView outputNode = (SimpleNodeView)edge.output.node;

                    //aview.portData.Forward[edge.input].linked = null;
                    inputNode.portData.Forward[edge.input].UnLink(outputNode.portData.Forward[edge.output]);
                }
            }
        }

        if (graphViewChange.edgesToCreate != null)
        {
            foreach (Edge e in graphViewChange.edgesToCreate)
            {
                SimpleNodeView inputView = (SimpleNodeView)e.input.node;
                SimpleNodeView outputView = (SimpleNodeView)e.output.node;

                PortView inputPortView = inputView.GetPortView(e.input);

                inputPortView.ConnectPort(outputView.GetPortView(e.output));
                //AssetDatabase.AddObjectToAsset(inputView.GetPortView(e.input), graph);
                EditorUtility.SetDirty(inputPortView);
                EditorUtility.SetDirty(inputView.node);
                AssetDatabase.SaveAssets();
            }
        }

        graph.DetectChange();

        return graphViewChange;
    }

    protected void CreatePropertyNode(int index)
    {
        Debug.Log("Property index: " + index);
        Node node = graph.CreatePropertyNode(index);
        if (node == null)
            throw new Exception("Node is null.");
        CreateNodeView(node);
    }

    protected void CreateNode(Type type, Vector2 position = new Vector2())
    {
        Node node = graph.CreateNode(type);
        if (node == null)
            throw new Exception("Node is null.");
        node.nodePosition = position;
        CreateNodeView(node);
    }

    void CreateNodeView(Node node)
    {
        SimpleNodeView nodeView = new SimpleNodeView(node, this);
        nodeView.OnNodeSelected = OnNodeSelected;
        AddElement(nodeView);
    }

    public void SaveAssetsButton()
    {
        if (graph == null)
            return;

        graph.SaveAsset();
    }
}