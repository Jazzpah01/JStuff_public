using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;

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

        public virtual Link.Direction InputPortDirection => Link.Direction.Input;
        public virtual Link.Orientation Orientation => Link.Orientation.Horizontal;



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
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                EditorUtility.SetDirty(sharedContext);
                AssetDatabase.AddObjectToAsset(sharedContext, this);
            }
#endif

            uniqueContext = CreateInstance<Context>();
            uniqueContext.name = "UniqueContext";
            uniqueContext.graph = this;
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                EditorUtility.SetDirty(uniqueContext);
                AssetDatabase.AddObjectToAsset(uniqueContext, this);
            }
#endif

            SetupProperties();

            isSetup = true;
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                AssetDatabase.SaveAssets();
            }
#endif
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
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                    {
                        EditorUtility.SetDirty(sharedContext);
                        AssetDatabase.AddObjectToAsset(sharedContext, this);
                    }
#endif
                }

                if (uniqueContext == null)
                {
                    uniqueContext = CreateInstance<Context>();
                    uniqueContext.name = "UniqueContext";
                    uniqueContext.graph = this;
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                    {
                        EditorUtility.SetDirty(uniqueContext);
                        AssetDatabase.AddObjectToAsset(uniqueContext, this);
                    }
#endif
                }

                sharedContext.Clear();
                uniqueContext.Clear();
                SetupProperties();

#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    AssetDatabase.SaveAssets();
                }
#endif
            }
        }

        protected virtual void Initialize()
        {

        }

        protected virtual void SetupProperties()
        {

        }

        public void CleanParentlessPortView()
        {
            HashSet<PortView> parentfulViews = new HashSet<PortView>();

            foreach (Node node in nodes)
            {
                foreach (PortView view in node.portViews)
                {
                    parentfulViews.Add(view);
                }
            }

            foreach (PortView view in portViews.ToArray())
            {
                if (!parentfulViews.Contains(view))
                {
#if UNITY_EDITOR
                    // Illigal view to delete
                    if (!Application.isPlaying)
                        AssetDatabase.RemoveObjectFromAsset(view);
#endif
                    portViews.Remove(view);
                }
            }
        }

        public void CleanupPortViews()
        {
            foreach (PortView view in portViews.ToArray())
            {
                if (view == null)
                {
                    portViews.Remove(view);
                    continue;
                }
                foreach (PortView linked in view.linked.ToArray())
                {
                    if (linked == null)
                    {
                        view.UnLinkAll();
                        continue;
                    }
                    if (!linked.Valid || !view.Valid)
                    {
                        view.UnLink(linked);
                    }
                }
            }

            for (int i = 0; i < portViews.Count; i++)
            {
                if (!portViews[i].Valid)
                {
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                        AssetDatabase.RemoveObjectFromAsset(portViews[i]);
                    portViews.Remove(portViews[i]);
#endif
                    i--;
                }
            }

            for (int i = 0; i < nodes.Count; i++)
            {
                if (!nodes[i].Valid)
                {
                    Node invalidNode = nodes[i];

                    nodes.Remove(invalidNode);

                    CreateNode(invalidNode.Clone());
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                        AssetDatabase.RemoveObjectFromAsset(invalidNode);
#endif
                    i--;
                }
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                AssetDatabase.SaveAssets();
            }
#endif
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
                throw new Exception("Can only initialize graph in runtime.");

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
#if UNITY_EDITOR
            node.guid = GUID.Generate().ToString();
#endif
            node.graph = this;
            node.UpdateNode();
            nodes.Add(node);
            node.Valid = true;

            if (type == RootNodeType)
            {
                rootNode = node;
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    EditorUtility.SetDirty(this);
                    AssetDatabase.SaveAssets();
                }
#endif
            }

            foreach (PortView portView in node.portViews)
            {
                portViews.Add(portView);
            }
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                AssetDatabase.AddObjectToAsset(node, this);
                AssetDatabase.SaveAssets();
            }
#endif
            return node;
        }

        public Node CreateNode(Node node)
        {
            node.name = node.GetType().Name;
#if UNITY_EDITOR
            node.guid = GUID.Generate().ToString();
#endif
            node.graph = this;
            node.isSetup = false;
            node.UpdateNode();
            nodes.Add(node);
            node.Valid = true;

            if (node.GetType() == RootNodeType)
            {
                rootNode = node;
                if (!Application.isPlaying)
                {
#if UNITY_EDITOR
                    EditorUtility.SetDirty(this);
                    AssetDatabase.SaveAssets();
#endif
                }
            }
            foreach (PortView portView in node.portViews)
            {
                portViews.Add(portView);
            }
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                AssetDatabase.AddObjectToAsset(node, this);
                AssetDatabase.SaveAssets();
            }
#endif
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
            CleanupPortViews();
            CleanParentlessPortView();
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                AssetDatabase.SaveAssets();
            }
#endif
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
#if UNITY_EDITOR
                AssetDatabase.RemoveObjectFromAsset(portView);
#endif
            }
#if UNITY_EDITOR
            AssetDatabase.RemoveObjectFromAsset(node);
            AssetDatabase.SaveAssets();
#endif
        }

        public void DetectChange()
        {
#if UNITY_EDITOR
            AssetDatabase.SaveAssets();
#endif
        }

        public void SaveAsset()
        {
#if UNITY_EDITOR
            AssetDatabase.SaveAssets();
#endif
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
#if UNITY_EDITOR
            if (!Application.isPlaying)
                AssetDatabase.SaveAssets();
#endif
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
#if UNITY_EDITOR
            if (!Application.isPlaying)
                AssetDatabase.SaveAssets();
#endif
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
                    try
                    {
                        portViews[i].ConnectPort(portViews[connection]);
                    }
                    catch (ArgumentException e)
                    {
                        portViews[i].UnLinkAll();
                        Debug.LogError("Error while connecting ports: " + e);
                    }
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
                    if (nodes[i].links[j].direction == Link.Direction.None)
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
            Debug.Log("resetting ports!");
            List<List<int>> connections = GetConnections();

            List<Node> newNodes = new List<Node>();
            for (int i = 0; i < nodes.Count; i++)
            {
                newNodes.Add(nodes[i].Clone());
            }
            isSetup = false;
            Clear();
            Setup();

            CreateNodes(newNodes);

            ConnectPorts(connections);
#if UNITY_EDITOR
            if (!Application.isPlaying)
                AssetDatabase.SaveAssets();
#endif
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