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
        public virtual bool Collapsable => false;
        [SerializeReference] public List<Node> nodes = new List<Node>();
        [System.NonSerialized] public List<Link> links = new List<Link>();

        public abstract Type[] NodeTypes { get; }

        public virtual Link.Direction InputPortDirection => Link.Direction.Input;
        public virtual Link.Orientation Orientation => Link.Orientation.Horizontal;



        [NonSerialized] public bool ProperyChanged = true;
        public int numberOfProperties;

        public bool isSetup = false;
        public bool initialized = false;

        public bool valid = false;

        public List<string> propertyValues = new List<string>();
        private PropertySetup propertySetup = PropertySetup.Editor;

        public List<Port> GetPorts()
        {
            List<Port> retval = new List<Port>();

            foreach (Node node in nodes)
            {
                if (node == null)
                    continue;

                foreach (Port port in node.ports)
                {
                    retval.Add(port);
                }
            }

            return retval;
        }

        public bool Valid { 
            get
            {
                if (!valid)
                    return false;

                foreach (Node node in nodes)
                {
                    if (node == null || !node.Valid)
                        return false;
                }

                return true;
            } 
        }

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

            if (sharedContext == null)
            {
                sharedContext = CreateInstance<Context>();
                sharedContext.name = "SharedContext";
                sharedContext.graph = this;
            } else
            {
                sharedContext.Clear();
            }

            if (uniqueContext == null)
            {
                uniqueContext = CreateInstance<Context>();
                uniqueContext.name = "UniqueContext";
                uniqueContext.graph = this;
            } else
            {
                uniqueContext.Clear();
            }

            SetupProperties();

            isSetup = true;
            valid = true;
        }

        public void UpdateGraph()
        {
            RefreshNodes();

            if (sharedContext == null)
            {
                valid = false;
                //sharedContext = CreateInstance<Context>();
                //sharedContext.name = "SharedContext";
                //sharedContext.graph = this;
            }

            if (uniqueContext == null)
            {
                valid = false;
                //uniqueContext = CreateInstance<Context>();
                //uniqueContext.name = "UniqueContext";
                //uniqueContext.graph = this;
            }

            foreach (Node node in nodes)
            {
                if (!node.Valid)
                {
                    node.UpdateNode();
                }
            }

            sharedContext.Clear();
            uniqueContext.Clear();
            SetupProperties();
        }

        protected virtual void Initialize()
        {

        }

        protected virtual void SetupProperties()
        {

        }


        protected PropertyLink<T> AddProperty<T>(T value, string name, PropertyContext context, bool isConstant = false)
        {
            if ((uniqueContext.Contains(name) || sharedContext.Contains(name)) && !sharedContext.Runmode)
                throw new Exception("Property of the specified name already exists: " + name);

            switch (context)
            {
                case PropertyContext.Unique:
                    uniqueContext.AddPropertyLink(value, name, isConstant);
                    if (uniqueContext.Runmode)
                    {
                        return (PropertyLink<T>)uniqueContext.GetPropertyLink(name);
                    }
                    break;
                case PropertyContext.Shared:
                    if (!sharedContext.Runmode)
                    {
                        sharedContext.AddPropertyLink(value, name, isConstant);
                    }
                    else if (sharedContext.Runmode && !sharedContext.Contains(name))
                    {
                        sharedContext.AddPropertyLink(value, name, isConstant);
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

            sharedContext.Runmode = true;
            uniqueContext.Runmode = true;
            SetupProperties();

            List<List<int>> connections = GetConnections(GetPorts());

            CreateAndConnectLinks(connections);
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

            node.graph = this;
            node.UpdateNode();
            nodes.Add(node);
            node.Valid = true;

            if (type == RootNodeType)
            {
                rootNode = node;
            }

            return node;
        }

        public Node AddNode(Node node)
        {
            node.name = node.GetType().Name;
            node.graph = this;
            node.isSetup = false;

            node.UpdateNode();

            if (!nodes.Contains(node))
                nodes.Add(node);

            node.Valid = true;

            if (node.GetType() == RootNodeType)
            {
                rootNode = node;
            }

            return node;
        }

        public void RefreshProperties()
        {
            if (sharedContext != null)
                sharedContext.Clear();
            if (uniqueContext != null)
                uniqueContext.Clear();
            if (uniqueContext != null || sharedContext != null)
                SetupProperties();
        }

        public void RefreshNodes()
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                Node node = nodes[i];

                if (node == null)
                {
                    nodes.RemoveAt(i);
                    i--;
                }

#if UNITY_EDITOR
                if (!AssetDatabase.IsSubAsset(node))
                {
                    nodes.RemoveAt(i);
                    i--;
                }
#endif

                if (node.SignatureChanged)
                {
                    node.Valid = false;
                }

                if (node.ports == null || node.ports.Count == 0)
                {
                    node.Valid = false;
                    return;
                }

                foreach (Port port in node.ports)
                {
                    Port[] ports = port.GetConnectedPorts();

                    if (ports == null || !port.Valid)
                    {
                        port.UnLinkAll();
                        continue;
                    }

                    foreach (Port connectedPort in port.GetConnectedPorts())
                    {
                        if (!connectedPort.Valid)
                        {
                            port.UnLinkAll();
                        }
                    }
                }
            }
            foreach (Node node in nodes)
            {
                
            }
        }

        public void DeleteNode(Node node)
        {
            if (node == rootNode)
            {
                rootNode = null;
            }
            nodes.Remove(node);
        }

        public virtual Graph Clone()
        {
            Graph retval = CreateInstance(this.GetType()) as Graph;
            retval.Setup();
            retval.name = name + "(Clone)";

            List<Port> ports = GetPorts();

            List<List<int>> connections = GetConnections(ports);

            List<Node> newNodes = new List<Node>();
            for (int i = 0; i < nodes.Count; i++)
            {
                newNodes.Add(nodes[i].Clone());
            }

            retval.CreateNodes(newNodes);

            retval.ConnectPorts(retval.GetPorts(), connections);

            retval.isSetup = true;
            return retval;
        }

        public List<List<int>> GetConnections(List<Port> ports)
        {
            for (int i = 0; i < ports.Count; i++)
            {
                ports[i].graphIndex = i;
            }
            List<List<int>> connections = new List<List<int>>();
            int conn = 0;
            for (int i = 0; i < ports.Count; i++)
            {
                connections.Add(new List<int>());
                Port[] connectedPorts = ports[i].GetConnectedPorts();
                for (int j = 0; j < connectedPorts.Length; j++)
                {
                    connections[i].Add(connectedPorts[j].graphIndex);
                    conn++;
                }
            }
            return connections;
        }

        public void CreateNodes(List<Node> newNodes)
        {
            nodes.Clear();
            rootNode = null;

            for (int i = 0; i < newNodes.Count; i++)
            {
                AddNode(newNodes[i]);
            }
        }

        public void ReCreateNodes(List<Node> oldNodes)
        {
            nodes.Clear();
            rootNode = null;

            for (int i = 0; i < oldNodes.Count; i++)
            {
                AddNode(oldNodes[i].Clone());
            }
        }

        public void ConnectPorts(List<Port> ports, List<List<int>> connections)
        {
            if (connections.Count > ports.Count)
            {
                valid = false;
                return;
            }

            for (int i = 0; i < connections.Count; i++)
            {
                ports[i].UnLinkAll();
                foreach (int connection in connections[i])
                {
                    try
                    {
                        ports[i].ConnectPort(ports[connection]);
                    }
                    catch (ArgumentException e)
                    {
                        ports[i].UnLinkAll();
                        Debug.LogError("Error while connecting ports: " + e);
                    }
                }
            }
        }

        public void CreateAndConnectLinks(List<List<int>> connections)
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
            List<Port> ports = GetPorts();
            List<List<int>> connections = GetConnections(ports);

            List<Node> newNodes = new List<Node>();
            for (int i = 0; i < nodes.Count; i++)
            {
                newNodes.Add(nodes[i].Clone());
            }
            isSetup = false;
            Clear();
            Setup();

            CreateNodes(newNodes);

            ConnectPorts(ports, connections);
        }

        public void Clear()
        {
            rootNode = null;

            DestroyImmediate(uniqueContext, true);
            DestroyImmediate(sharedContext, true);

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
            List<Port> ports = GetPorts();
            retval += $"PortViews ({ports.Count}):\r\n";
            for (int i = 0; i < ports.Count; i++)
            {
                retval += $"Portview: {ports[i].name}. GUID of node: {ports[i].node.guid}\r\n";
            }

            return retval;
        }

        public Graph GetInitializedGraph()
        {
            // Cloning graph
            Graph retval = Clone();

            // Setup properties
            //retval.sharedContext = sharedContext;
            retval.sharedContext.Runmode = true;
            retval.uniqueContext.Runmode = true;
            retval.SetupProperties();

            // Get connections from old graph
            List<List<int>> connections = GetConnections(GetPorts());

            // Connect links
            retval.CreateAndConnectLinks(connections);

            // User graph initialization
            retval.Initialize();

            // User node initialization
            foreach (Node node in retval.nodes)
            {
                node.Initialize();
            }

            if (retval.Collapsable)
                retval.CollapseGraph();

            retval.initialized = true;

            return retval;
        }

        public Graph GetChild()
        {
            if (!initialized)
            {
                throw new Exception("Cannot get child of uninitialized graph!");
            }

            // Cloning graph
            Graph retval = Clone();

            // Setup properties
            retval.sharedContext = sharedContext;
            retval.sharedContext.Runmode = true;
            retval.uniqueContext.Runmode = true;
            retval.SetupProperties();

            // Get connections from old graph
            List<List<int>> connections = GetConnections(GetPorts());

            // Connect links
            retval.CreateAndConnectLinks(connections);

            // User graph initialization
            retval.Initialize();

            // User node initialization
            foreach (Node node in retval.nodes)
            {
                node.Initialize();
            }

            // User node initialization
            for (int i = 0; i < retval.nodes.Count; i++)
            {
                retval.nodes[i].Initialize();

                for (int j = 0; j < nodes[i].links.Count; j++)
                {
                    if (nodes[i].links[j] is ILinkable && nodes[i].links[j].IsConstant() && ((ILinkable)nodes[i].links[j]).LinkedPort != null)
                    {


                        //Debug.Log(nodes[i].links[j].GetType());
                        //Debug.Log(nodes[i].links[j].node);

                        ILinkable other = (ILinkable)nodes[i].links[j];
                        ILinkable thisl = (ILinkable)retval.nodes[i].links[j];

                        //Debug.Log(((Link)other.LinkedPort).node);
                        //Debug.Log(other.LinkedPort.GetType());

                        thisl.LinkPort((Link)other.LinkedPort);

                        //retval.nodes[i].links[j] = nodes[i].links[j];
                    }
                }
            }

            retval.initialized = true;

            return retval;
        }

        public void CollapseGraph()
        {
            foreach (Node node in nodes)
            {
                if (!node.IsConstant())
                {
                    foreach (Link link in node.links)
                    {
                        if (link.IsInput && link.node != null && link.IsConstant())
                        {
                            link.Collapse();
                        }
                    }
                }
            }
        }
    }
}