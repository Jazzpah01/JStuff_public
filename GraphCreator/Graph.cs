using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

namespace JStuff.GraphCreator
{
    public abstract class Graph : ScriptableObject
    {
        [SerializeReference] public Node rootNode;
        public abstract Type RootNodeType { get; }
        [SerializeReference] public List<Node> nodes = new List<Node>();
        [SerializeReference] public List<PortView> portViews = new List<PortView>();
        [System.NonSerialized] public List<Link> links = new List<Link>();

        public abstract List<Type> NodeTypes { get; }

        public virtual Direction InputPortDirection => Direction.Input;
        public virtual Orientation Orientation => Orientation.Horizontal;



        [NonSerialized] public bool ProperyChanged = true;
        public int numberOfProperties;

        public bool isSetup = false;
        public bool initialized = false;

        public bool valid = false;

        public List<string> propertyValues = new List<string>();
        private PropertySetup propertySetup = PropertySetup.Editor;
        private enum PropertySetup
        {
            Runtime,
            Editor
        }

        public enum PropertyContext
        {
            Unique,
            Shared
        }

        public Context uniqueContext;
        public Context sharedContext;

        public void Setup()
        {
            isSetup = false;

            Clear();

            sharedContext = CreateInstance<Context>();
            sharedContext.name = "SharedContext";
            sharedContext.graph = this;
            if (!Application.isPlaying)
            {
                EditorUtility.SetDirty(sharedContext);
                AssetDatabase.AddObjectToAsset(sharedContext, this);
            }

            uniqueContext = CreateInstance<Context>();
            uniqueContext.name = "UniqueContext";
            uniqueContext.graph = this;
            if (!Application.isPlaying)
            {
                EditorUtility.SetDirty(uniqueContext);
                AssetDatabase.AddObjectToAsset(uniqueContext, this);
            }

            SetupProperties();

            isSetup = true;

            if (!Application.isPlaying)
            {
                AssetDatabase.SaveAssets();
            }
        }

        public void UpdateGraph()
        {
            if (!isSetup)
            {
                Setup();
            }
            else
            {
                if (sharedContext == null)
                {
                    sharedContext = CreateInstance<Context>();
                    sharedContext.name = "SharedContext";
                    sharedContext.graph = this;
                    if (!Application.isPlaying)
                    {
                        EditorUtility.SetDirty(sharedContext);
                        AssetDatabase.AddObjectToAsset(sharedContext, this);
                    }
                }

                if (uniqueContext == null)
                {
                    uniqueContext = CreateInstance<Context>();
                    uniqueContext.name = "UniqueContext";
                    uniqueContext.graph = this;
                    if (!Application.isPlaying)
                    {
                        EditorUtility.SetDirty(uniqueContext);
                        AssetDatabase.AddObjectToAsset(uniqueContext, this);
                    }
                }

                sharedContext.Clear();
                uniqueContext.Clear();
                SetupProperties();

                if (!Application.isPlaying)
                {
                    AssetDatabase.SaveAssets();
                }
            }
        }

        protected virtual void Initialize()
        {

        }

        protected virtual void SetupProperties()
        {

        }

        public void CleanupPortViews()
        {

            foreach (PortView view in portViews.ToArray())
            {
                if (view == null)
                {
                    continue;
                }
                foreach (PortView linked in view.linked.ToArray())
                {
                    if (linked == null)
                    {
                        continue;
                    }
                    if (!linked.Valid)
                    {
                        view.UnLink(linked);
                    }
                }
            }


            for (int i = 0; i < portViews.Count; i++)
            {
                if (!portViews[i].Valid)
                {
                    if (!Application.isPlaying)
                        AssetDatabase.RemoveObjectFromAsset(portViews[i]);
                    portViews.Remove(portViews[i]);
                    i--;
                }
            }

            //List<(Type, Vector2)> nodesToRecreate = new List<(Type, Vector2)>();

            for (int i = 0; i < nodes.Count; i++)
            {
                if (!nodes[i].Valid)
                {
                    Node invalidNode = nodes[i];
                    
                    while(nodes[i].portViews != null && nodes[i].portViews.Count > 0)
                    {
                        PortView view = nodes[i].portViews[0];
                        nodes[i].portViews.Remove(view);
                        if (portViews.Contains(view))
                        {
                            portViews.Remove(view);
                        }
                        if (!Application.isPlaying)
                            AssetDatabase.RemoveObjectFromAsset(view);
                    }

                    nodes.Remove(invalidNode);
                    nodes.Add(invalidNode);
                    invalidNode.isSetup = false;
                    invalidNode.UpdateNode();

                    i--;

                    //nodesToRecreate.Add((invalidNode.GetType(), invalidNode.nodePosition));
                    //nodes.Remove(invalidNode);
                    //if (!Application.isPlaying)
                    //    AssetDatabase.RemoveObjectFromAsset(portViews[i]);

                }
            }

            //for (int i = 0; i < nodesToRecreate.Count; i++)
            //{
            //    Node newNode = CreateNode(nodesToRecreate[i].Item1);
            //    newNode.nodePosition = nodesToRecreate[i].Item2;
            //}

            if (!Application.isPlaying)
            {
                AssetDatabase.SaveAssets();
            }
        }

        protected PropertyLink<T> AddProperty<T>(T value, string name, PropertyContext context)
        {
            if ((uniqueContext.Contains(name) || sharedContext.Contains(name)) && !Application.isPlaying)
                throw new Exception("Property of the specified name already exists: " + name);

            switch (context)
            {
                case PropertyContext.Unique:
                    uniqueContext.AddPropertyLink(value, name);
                    if (uniqueContext.runmode)
                    {
                        return (PropertyLink<T>)uniqueContext.GetPropertyLink(name);
                    }
                    break;
                case PropertyContext.Shared:
                    sharedContext.AddPropertyLink(value, name);
                    if (sharedContext.runmode)
                    {
                        return (PropertyLink<T>)sharedContext.GetPropertyLink(name);
                    }
                    break;
            }
            return null;
        }

        public string GetPropertyType(string propertyName)
        {
            if (uniqueContext.Contains(propertyName))
            {
                return uniqueContext.GetPropertyType(propertyName);
            }
            else
            if (sharedContext.Contains(propertyName))
            {
                return sharedContext.GetPropertyType(propertyName);
            }
            else
            {
                throw new Exception("Property with the specified name doesn't exist: " + propertyName);
            }
        }

        public Link GetProperty(string name)
        {
            if (uniqueContext.Contains(name))
            {
                return uniqueContext.GetPropertyLink(name);
            }
            else if (sharedContext.Contains(name))
            {
                return sharedContext.GetPropertyLink(name);
            }
            else
            {
                throw new Exception("Property with the specified name doesn't exist: " + name);
            }
        }

        public void InitializeGraph()
        {
            if (!Application.isPlaying)
                throw new Exception("Can only setup graph in runtime.");

            sharedContext.runmode = true;
            uniqueContext.runmode = true;
            SetupProperties();
            foreach (Node node in nodes)
            {
                if (node.graph != this)
                {
                    throw new Exception("Node dependencies are currupt");
                }
                node.UpdateNode();
            }

            List<List<int>> connections = GetConnections();

            ConnectLinks(connections);
            Initialize();

            foreach (Node node in nodes)
            {
                node.Initialize();
            }

            initialized = true;
        }

        

        public Node CreateNode(Type type)
        {
            if (type == RootNodeType && rootNode != null)
            {
                throw new Exception("Cannot add multiple root nodes.");
            }

            Node node = CreateInstance(type) as Node;
            node.name = type.Name;
            node.guid = GUID.Generate().ToString();
            node.graph = this;
            node.UpdateNode();
            nodes.Add(node);
            node.Valid = true;

            if (type == RootNodeType)
            {
                rootNode = node;
                if (!Application.isPlaying)
                {
                    EditorUtility.SetDirty(this);
                    AssetDatabase.SaveAssets();
                }
            }

            foreach (PortView portView in node.portViews)
            {
                portViews.Add(portView);
            }

            if (!Application.isPlaying)
            {
                AssetDatabase.AddObjectToAsset(node, this);
                AssetDatabase.SaveAssets();
            }

            return node;
        }

        public Node CreateNode(Node node)
        {
            node.name = node.GetType().Name;
            node.guid = GUID.Generate().ToString();
            node.graph = this;
            node.UpdateNode();
            nodes.Add(node);
            node.Valid = true;

            if (node.GetType() == RootNodeType)
            {
                rootNode = node;
                if (!Application.isPlaying)
                {
                    EditorUtility.SetDirty(this);
                    AssetDatabase.SaveAssets();
                }
            }
            foreach (PortView portView in node.portViews)
            {
                portViews.Add(portView);
            }

            if (!Application.isPlaying)
            {
                AssetDatabase.AddObjectToAsset(node, this);
                AssetDatabase.SaveAssets();
            }

            return node;
        }

        public void UpdateNodes()
        {
            foreach (Node node in nodes)
            {
                if (node.SignatureChanged)
                {
                    node.Valid = false;
                }
            }
            UpdateGraph();
            CleanupPortViews(); if (!Application.isPlaying)
            {
                AssetDatabase.SaveAssets();
            }
        }

        public void DeleteNode(Node node)
        {
            if (node == rootNode)
            {
                rootNode = null;
            }
            nodes.Remove(node);

            foreach (PortView portView in node.portViews)
            {
                portViews.Remove(portView);

                AssetDatabase.RemoveObjectFromAsset(portView);
            }

            AssetDatabase.RemoveObjectFromAsset(node);
            AssetDatabase.SaveAssets();
        }

        public void DetectChange()
        {
            AssetDatabase.SaveAssets();
        }

        public void SaveAsset()
        {
            AssetDatabase.SaveAssets();
        }

        public virtual Graph Clone()
        {
            Graph retval = CreateInstance(this.GetType()) as Graph;
            retval.Setup();
            retval.name = name + "(Clone)";

            List<List<int>> connections = GetConnections();

            List<Node> newNodes = new List<Node>();
            for (int i = 0; i < nodes.Count; i++)
            {
                newNodes.Add(nodes[i].Clone());
            }

            retval.CreateNodes(newNodes);

            retval.ConnectPorts(connections);

            retval.isSetup = true;
            return retval;
        }

        public List<List<int>> GetConnections()
        {
            for (int i = 0; i < portViews.Count; i++)
            {
                portViews[i].graphIndex = i;
            }
            List<List<int>> connections = new List<List<int>>();
            for (int i = 0; i < portViews.Count; i++)
            {
                connections.Add(new List<int>());
                for (int j = 0; j < portViews[i].linked.Count; j++)
                {
                    connections[i].Add(portViews[i].linked[j].graphIndex);
                }
            }
            return connections;
        }

        public void CreateNodes(List<Node> newNodes)
        {
            portViews.Clear();
            nodes.Clear();
            rootNode = null;

            for (int i = 0; i < newNodes.Count; i++)
            {
                CreateNode(newNodes[i]);
            }
        }

        public void ReCreateNodes(List<Node> oldNodes)
        {
            portViews.Clear();
            nodes.Clear();
            rootNode = null;

            for (int i = 0; i < oldNodes.Count; i++)
            {
                CreateNode(oldNodes[i].Clone());
            }
        }

        public void ConnectPorts(List<List<int>> connections)
        {
            if (connections.Count > portViews.Count)
            {
                valid = false;
                return;
            }

            for (int i = 0; i < connections.Count; i++)
            {
                portViews[i].UnLinkAll();
                foreach (int connection in connections[i])
                {
                    portViews[i].ConnectPort(portViews[connection]);
                }
            }
        }

        public void ConnectLinks(List<List<int>> connections)
        {
            int graphIndex = 0;
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].SetupLinks();
                for (int j = 0; j < nodes[i].links.Count; j++)
                {
                    if (nodes[i].links[j].Direction == Direction.None)
                        continue;

                    Link link = nodes[i].links[j];
                    link.graphIndex = graphIndex;
                    links.Add(link);
                    graphIndex++;
                }
            }
            for (int i = 0; i < connections.Count; i++)
            {
                foreach (int connection in connections[i])
                {
                    ((ILinkable)links[i]).LinkPort(links[connection]);
                }
            }
        }

        public void ResetPorts()
        {
            List<List<int>> connections = GetConnections();

            List<Node> newNodes = new List<Node>();
            for (int i = 0; i < nodes.Count; i++)
            {
                newNodes.Add(nodes[i].Clone());
            }
            isSetup = false;
            Setup();

            CreateNodes(newNodes);

            ConnectPorts(connections);
        }

        public void Clear()
        {
            rootNode = null;

            DestroyImmediate(uniqueContext, true);
            DestroyImmediate(sharedContext, true);

            while (portViews.Count > 0)
            {
                PortView p = portViews[0];
                portViews.RemoveAt(0);
                DestroyImmediate(p, true);
            }
            portViews.Clear();

            while (nodes.Count > 0)
            {
                Node n = nodes[0];
                nodes.RemoveAt(0);
                DestroyImmediate(n, true);
            }
            nodes.Clear();
        }

        public override string ToString()
        {
            string retval = "";

            retval += $"GraphCreator Graph. Name: {this.name}\r\n";

            retval += $"Root node: {rootNode.name}\r\n";

            retval += $"Nodes ({nodes.Count}):\r\n";

            for (int i = 0; i < nodes.Count; i++)
            {
                retval += $"Node: {nodes[i].name}. GUID: {nodes[i].guid}\r\n";
            }
            retval += $"PortViews ({portViews.Count}):\r\n";
            for (int i = 0; i < portViews.Count; i++)
            {
                retval += $"Portview: {portViews[i].name}. GUID of node: {portViews[i].node.guid}\r\n";
            }

            return retval;
        }
    }
}