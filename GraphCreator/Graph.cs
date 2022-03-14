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

        public abstract List<Type> NodeTypes { get; }

        public virtual Direction InputPortDirection => Direction.Input;
        public virtual Orientation Orientation => Orientation.Horizontal;


        public List<PortView> propertyPortViews = new List<PortView>();
        public List<Link> propertyPorts = new List<Link>();
        public List<object> oldpropertyValues = new List<object>();
        public List<string> propertyNames = new List<string>();
        public List<string> propertyTypeNames = new List<string>();
        private Dictionary<string, int> propertyIndexMap = new Dictionary<string, int>();

        [NonSerialized] public bool ProperyChanged = true;
        public int numberOfProperties;

        public bool isSetup = false;
        public bool initialized = false;

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



        public object GetProperty(int index)
        {
            return oldpropertyValues[index];
        }

        public Link GetPropertyPort(int index)
        {
            Debug.Log(propertyPorts);
            Debug.Log(propertyPorts.Count);

            return propertyPorts[index];
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
            if (!Application.isPlaying)
                AssetDatabase.SaveAssets();
        }

        protected PropertyPort<T> AddProperty<T>(T value, string name, PropertyContext context)
        {
            if ((uniqueContext.Contains(name) || sharedContext.Contains(name)) && !Application.isPlaying)
                throw new Exception("Property of the specified name already exists: " + name);

            switch (context)
            {
                case PropertyContext.Unique:
                    uniqueContext.AddPropertyLink(value, name);
                    if (uniqueContext.runmode)
                    {
                        return (PropertyPort<T>)uniqueContext.GetPropertyLink(name);
                    }
                    break;
                case PropertyContext.Shared:
                    sharedContext.AddPropertyLink(value, name);
                    if (sharedContext.runmode)
                    {
                        return (PropertyPort<T>)sharedContext.GetPropertyLink(name);
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
            ConnectLinks();
            Initialize();

            foreach (Node node in nodes)
            {
                node.Initialize();
            }

            initialized = true;
        }

        public Node CreatePropertyNode(int propertyIndex)
        {
            PropertyNode node = CreateInstance<PropertyNode>() as PropertyNode;
            node.name = propertyNames[propertyIndex];
            node.guid = GUID.Generate().ToString();
            node.graph = this;
            //node.InitPropertyPortView(propertyIndex);
            nodes.Add(node);
            node.Valid = true;

            foreach (PortView portView in node.portViews)
            {
                portViews.Add(portView);
                EditorUtility.SetDirty(portView);
                AssetDatabase.AddObjectToAsset(portView, this);
            }

            AssetDatabase.AddObjectToAsset(node, this);
            AssetDatabase.SaveAssets();

            return node;
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
                    node.UpdateNode();
                    foreach (PortView view in node.portViews)
                    {
                        portViews.Add(view);
                    }
                }
            }
            if (!Application.isPlaying)
            {
                AssetDatabase.SaveAssets();
            }
            UpdateGraph();
            CleanupPortViews();
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
            for (int i = 0; i < connections.Count; i++)
            {
                portViews[i].UnLinkAll();
                foreach (int connection in connections[i])
                {
                    portViews[i].ConnectPort(portViews[connection]);
                }
            }
        }

        public void ConnectLinks()
        {
            foreach (Node node in nodes)
            {
                node.SetupLinks();
            }
            foreach (Node node in nodes)
            {
                node.ConnectLinks();
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