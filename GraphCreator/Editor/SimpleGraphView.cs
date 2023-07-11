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
using Port = JStuff.GraphCreator.Port;

public class SimpleGraphView : UnityEditor.Experimental.GraphView.GraphView
{
    public new class UxmlFactory : UxmlFactory<SimpleGraphView, UxmlTraits> { }

    public Graph graph;

    public Action<SimpleNodeView> OnNodeSelected;

    SimpleNodeView selectedNode;

    public SimpleGraphView()
    {
        Insert(0, new GridBackground());

        this.AddManipulator(new ContentZoomer());
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/JStuff_public/GraphCreator/Editor/SimpleGraphEditor.uss");
        styleSheets.Add(styleSheet);

        Undo.undoRedoPerformed += UndoRedoPerformed;
    }

    private void UndoRedoPerformed()
    {
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        if (graph == null)
        {
            Debug.LogWarning("Cannot edit without a graph selected.");
            return;
        }

        //evt.menu.AppendAction("Reload Graph", delegate { graph.ResetPorts(); PopulateView(graph); OnNodeSelected(FindNodeView(graph.rootNode)); });

        evt.menu.AppendAction("Save Graph", delegate { SaveAssetsButton(); });

        if (selection != null && selection.Count == 1 && selection[0] is SimpleNodeView)
        {
            Node node = ((SimpleNodeView)selection[0]).node;
            evt.menu.AppendAction("Dublicate Node", delegate { DublicateNode(node); OnNodeSelected(FindNodeView(node)); });
        }

        foreach (Type type in graph.NodeTypes)
        {
            AddMenuType(evt, type);
        }
    }

    private void AddMenuType(ContextualMenuPopulateEvent evt, Type t)
    {
        if (t.IsAbstract)
        {
            var types = TypeCache.GetTypesDerivedFrom(t);
            foreach (var type in types)
            {
                AddMenuType(evt, type);
            }
            return;
        }

        CreateNodePath attribute =
            (CreateNodePath)Attribute.GetCustomAttribute(t, typeof(CreateNodePath));

        if (attribute == null)
            return;

        Vector2 localMousePos = evt.localMousePosition;
        Vector2 actualGraphPosition = viewTransform.matrix.inverse.MultiplyPoint(localMousePos);

        evt.menu.AppendAction($"{attribute.path}", (a) => CreateNode(t, actualGraphPosition));
    }

    public override List<UnityEditor.Experimental.GraphView.Port> GetCompatiblePorts(UnityEditor.Experimental.GraphView.Port startPort, NodeAdapter nodeAdapter)
    {
        return ports.ToList().Where(
            endPort =>
            startPort.direction != endPort.direction &&
            startPort.portType == endPort.portType).ToList();
    }

    internal void PopulateView(Graph graph)
    {
        if (graph == null)
            return;

        this.graph = graph;

        graphViewChanged -= OnGraphViewChanged;
        DeleteElements(graphElements.ToList());
        graphViewChanged += OnGraphViewChanged;

        // Check if graph is valid
        CheckGraphValidity();

        // Create node view
        if (!this.graph.isSetup)
        {
            Undo.RecordObject(this.graph, "Graph Creator (Setup)");

            graph.Setup();
            graph.sharedContext.guid = GUID.Generate().ToString();
            graph.uniqueContext.guid = GUID.Generate().ToString();

            Debug.Log(this.graph.sharedContext);

            AssetDatabase.AddObjectToAsset(graph.sharedContext, this.graph);
            AssetDatabase.AddObjectToAsset(graph.uniqueContext, this.graph);

            Undo.RegisterCreatedObjectUndo(graph.sharedContext, "Graph Creator (Setup)");
            Undo.RegisterCreatedObjectUndo(graph.uniqueContext, "Graph Creator (Setup)");

            AssetDatabase.SaveAssets();
        }

        foreach (Node node in this.graph.nodes)
        {
            CreateNodeView(node);
        }

        // Create root node
        if (this.graph.rootNode == null)
        {
            CreateNode(this.graph.RootNodeType);
        }

        // Set root nodeview properties
        FindNodeView(this.graph.rootNode).capabilities &= ~Capabilities.Deletable;

        // Create edges
        bool error = false;
        foreach (Node node in this.graph.nodes)
        {
            foreach (JStuff.GraphCreator.Port inputPort in node.ports)
            {
                try
                {
                    if (inputPort.node == null || inputPort.connectedNodes.Count < 1)
                        continue;

                    Node inputnode = inputPort.node;

                    foreach (JStuff.GraphCreator.Port outputPort in inputPort.GetConnectedPorts())
                    {
                        Node outputnode = outputPort.node;

                        SimpleNodeView inputNodeView = FindNodeView(inputnode);
                        SimpleNodeView outputNodeView = FindNodeView(outputnode);

                        Edge edge = inputNodeView.GetPort(inputPort).ConnectTo(outputNodeView.GetPort(outputPort));
                        edge.input.portType = outputPort.PortType;
                        edge.output.portType = outputPort.PortType;
                        AddElement(edge);
                    }
                } catch {
                    node.valid = false;
                    error = true;
                }
            }
        }

        if (error)
            CheckGraphValidity();
    }

    void CheckGraphValidity()
    {
        graph.RefreshProperties();
        graph.RefreshNodes();
        if (!graph.Valid)
        {
            for (int i = 0; i < graph.nodes.Count; i++)
            {
                Node node = graph.nodes[i];

                if (node == null)
                {
                    graph.nodes.RemoveAt(i);
                    i--;
                    continue;
                }

                if (!node.Valid)
                {
                    node.UpdateNode();
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
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
            foreach (GraphElement element in graphViewChange.elementsToRemove.ToArray())
            {
                SimpleNodeView view = element as SimpleNodeView;
                if (view != null)
                {
                    DeleteNode(view.node, true);
                }

                Edge edge = element as Edge;
                if (edge != null)
                {
                    SimpleNodeView inputNode = (SimpleNodeView)edge.input.node;
                    SimpleNodeView outputNode = (SimpleNodeView)edge.output.node;

                    JStuff.GraphCreator.Port inputPort = inputNode.GetPortView(edge.input);
                    Port outputPort = outputNode.GetPortView(edge.output);

                    Undo.RecordObject(inputNode.node, "Graph Creator (Disconnect ports)");
                    EditorUtility.SetDirty(inputNode.node);

                    inputPort.UnLink(outputPort);
                }
            }
        }

        if (graphViewChange.edgesToCreate != null)
        {
            foreach (Edge edge in graphViewChange.edgesToCreate)
            {
                SimpleNodeView inputView = (SimpleNodeView)edge.input.node;
                SimpleNodeView outputView = (SimpleNodeView)edge.output.node;

                JStuff.GraphCreator.Port inputPortView = inputView.GetPortView(edge.input);
                Port outputPort = outputView.GetPortView(edge.output);

                Undo.RecordObject(inputView.node, "Graph Creator (Connect ports)");
                EditorUtility.SetDirty(inputView.node);

                inputPortView.ConnectPort(outputView.GetPortView(edge.output));
            }
        }

        AssetDatabase.SaveAssets();

        return graphViewChange;
    }

    public void CreateNode(Type type, Vector2 position = new Vector2())
    {
        Undo.RegisterCompleteObjectUndo(graph, "Graph Creator (Create node)");
        Node node = graph.CreateNode(type);

        node.guid = GUID.Generate().ToString();

        AssetDatabase.AddObjectToAsset(node, graph);

        EditorUtility.SetDirty(node);

        Undo.RegisterCreatedObjectUndo(node, "Graph Creator (Create node)");

        Undo.FlushUndoRecordObjects();
        
        EditorUtility.SetDirty(graph);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        CreateNodeView(node, position);
    }

    public void AddNode(Node node, Vector2 position = new Vector2())
    {
        Debug.Log("addnode");
        EditorUtility.SetDirty(graph);

        Undo.RegisterCompleteObjectUndo(graph, "Graph Creator (Add node)");

        if (AssetDatabase.IsSubAsset(node))
        {
            CreateNodeView(node, position);
        }
        else
        {
            node.guid = GUID.Generate().ToString();

            AssetDatabase.AddObjectToAsset(node, graph);
            Undo.RegisterCreatedObjectUndo(node, "Graph Creator (Add node)");

            EditorUtility.SetDirty(node);

            Undo.FlushUndoRecordObjects();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            CreateNodeView(node, position);
        }
    }

    public Node UpdateNode(Node node)
    {
        Undo.RegisterCreatedObjectUndo(node, "Graph Creator (Update node)");

        node = graph.AddNode(node);

        Undo.FlushUndoRecordObjects();

        //AssetDatabase.SaveAssets();
        //AssetDatabase.Refresh();

        CreateNodeView(node, node.nodePosition);

        return node;
    }

    public void CreateNodeView(Node node, Vector2 position = new Vector2())
    {
        if (position != Vector2.zero)
            node.nodePosition = position;
        SimpleNodeView nodeView = new SimpleNodeView(node, this);
        nodeView.OnNodeSelected = OnNodeSelected;
        AddElement(nodeView);
    }

    public void DeleteNode(Node node, bool endingChange)
    {
        Undo.RegisterCompleteObjectUndo(graph, "Graph Creator (Delete Node)");

        Undo.DestroyObjectImmediate(node);

        graph.DeleteNode(node);

        Undo.FlushUndoRecordObjects();

        EditorUtility.SetDirty(graph);

        PopulateView(graph);
    }

    public void SaveAssetsButton()
    {
        if (graph == null)
            return;

        AssetDatabase.SaveAssets();
    }

    public void SaveGraph()
    {

    }

    public void SavePorts()
    {
        
    }

    public void SaveNodes()
    {

    }
    
    public void DublicateNode(Node node)
    {
        Node nnode = graph.AddNode(node.Clone());

        AddNode(nnode);

        if (nnode == null)
            throw new Exception("Node is null.");
        CreateNodeView(nnode, node.nodePosition + Vector2.one * 10);
    }

    public void NodeSelected(SimpleNodeView nodeView)
    {
        selectedNode = nodeView;
    }

    //public void CleanPortViews()
    //{
    //    foreach (Port port in graph.ports.ToArray())
    //    {
    //        if (port == null)
    //        {
    //            graph.ports.Remove(port);
    //            continue;
    //        }
    //        foreach (Port linked in port.linked.ToArray())
    //        {
    //            if (linked == null)
    //            {
    //                port.UnLinkAll();
    //                continue;
    //            }
    //            if (!linked.Valid || !port.Valid)
    //            {
    //                port.UnLink(linked);
    //            }
    //        }
    //    }

    //    for (int i = 0; i < graph.ports.Count; i++)
    //    {
    //        if (graph.ports[i] == null)
    //        {
    //            graph.ports.RemoveAt(i);
    //            i--;
    //            continue;
    //        }
    //        if (!graph.ports[i].Valid)
    //        {
    //            graph.ports.Remove(graph.ports[i]);

    //            i--;
    //        }
    //    }

    //    for (int i = 0; i < graph.nodes.Count; i++)
    //    {
    //        if (graph.nodes[i] == null)
    //        {
    //            graph.nodes.RemoveAt(i);
    //            i--;
    //            continue;
    //        }
    //        if (!graph.nodes[i].Valid)
    //        {
    //            Node invalidNode = graph.nodes[i];

    //            AddNode(invalidNode.Clone(), false);

    //            DeleteNode(graph.nodes[i], true);

    //            i--;
    //        }
    //    }

    //    if (!Application.isPlaying)
    //    {
    //        AssetDatabase.SaveAssets();
    //    }
    //}
}