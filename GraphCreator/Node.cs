using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace JStuff.GraphCreator
{
    [Serializable]
    public abstract class Node : ScriptableObject, ISimpleNode, IInvalid
    {
        [HideInInspector] public bool isSetup = false;

        [HideInInspector] public Vector2 nodePosition;
        [HideInInspector] public List<Link> links = new List<Link>();
        [HideInInspector] public List<Link> cacheLinks = new List<Link>();
        [HideInInspector] [SerializeReference] public List<PortView> portViews = new List<PortView>();
        [HideInInspector] public string guid;
        [HideInInspector] public Graph graph;

        public int iteration = 0;

        public string linkDescription = "";

        public virtual StyleSheet StyleSheet => null;

        public int Length => Mathf.Max(links.Count, portViews.Count);

        public virtual string path => this.GetType().Name;

        // Can use node.outputContainer/inputContainer for ports
        // Can use node.titleContainer for node
        public virtual void OnGUIStart(INodeView nodeView)
        {

        }

        public virtual void OnGUISelected(INodeView nodeView)
        {

        }

        public virtual void OnGUIUnselected(INodeView nodeView)
        {

        }

        public Action OnNodeChange;

        private bool valid = false;

        [Flags]
        public enum InputPortSettings
        {
            None = 0,
            Optional = 0b1
        }

        [Flags]
        public enum PortSetup
        {
            None = 0,
            Editor = 0b1,
            Signature = 0b10,
            Runtime = 0b100
        }

        public virtual bool CacheOutput => false;

        public bool SignatureChanged
        {
            get
            {
                return portSignature != CurrentPortSignature;
            }
        }

        private string CurrentPortSignature
        {
            get
            {
                string temp1 = portSignature;
                SetupPorts_Editor(InitPortStrategy.StringGeneration);
                string temp2 = portSignature;
                portSignature = temp1;
                return temp2;
            }
        }

        protected abstract void SetupPorts();
        protected virtual void SetupNode() { }
        public List<Link> Ports => links;

        public bool Valid
        {
            get => valid;
            set
            {
                valid = value;

                if (!value)
                {
                    foreach (PortView view in portViews)
                    {
                        view.Valid = false;
                    }
                }
            }
        }

        [SerializeField] private string portSignature = "";

        private InitPortStrategy currentStrategy = InitPortStrategy.None;

        private enum InitPortStrategy
        {
            None,
            StringGeneration,
            PortViewGeneration,
            LinkGeneration
        }

        public void UpdateNode()
        {
            if (!isSetup)
            {
                portSignature = "";
                SetupNode();
                SetupPorts_Editor(InitPortStrategy.StringGeneration);
                SetupPorts_Editor(InitPortStrategy.PortViewGeneration);
                isSetup = true;
                valid = true;
                return;
            }

            // Check for change in port signature
            string oldPortSignature = portSignature;
            portSignature = "";
            SetupPorts_Editor(InitPortStrategy.StringGeneration);

            if (oldPortSignature == portSignature)
            {
                // Unchanged port signatures
            }
            else
            {
                // New port signature: cleanup old port views
                foreach (PortView view in portViews)
                {
                    view.Valid = false;
                }

                valid = false;

                portViews = new List<PortView>();
                //SetupPorts_Editor(InitPortStrategy.PortViewGeneration);

                if (OnNodeChange != null)
                    OnNodeChange();
            }
        }

        public void SetupLinks()
        {
            // Application.isPlaying, then do that
            if (Application.isPlaying)
            {
                SetupPorts_Editor(InitPortStrategy.LinkGeneration);
                return;
            }
        }

        private void SetupPorts_Editor(InitPortStrategy strategy)
        {
            currentStrategy = strategy;
            if (strategy == InitPortStrategy.StringGeneration)
            {
                portSignature = "";
            }
            SetupPorts();
            currentStrategy = InitPortStrategy.None;
        }

        public virtual void Initialize()
        {

        }

        protected InputMultiLink<T> AddInputMultiLink<T>(string portName = "default",
            PortSetup portSetup = PortSetup.Editor | PortSetup.Signature | PortSetup.Runtime,
            InputPortSettings inputPortSettings = InputPortSettings.None)
        {
            Port.Capacity capacity = Port.Capacity.Multi;
            PortView portView;
            switch (currentStrategy)
            {
                case InitPortStrategy.None when (portSetup & PortSetup.Editor) != 0:
                    // Editor port view
                    portView = CreateInstance<PortView>();
                    AddPortView(portView, typeof(T).FullName, capacity, graph.InputPortDirection, portName);
                    break;
                case InitPortStrategy.None:
                    throw new Exception("Cannot setup ports with None strategy!");
                case InitPortStrategy.StringGeneration when (portSetup & PortSetup.Signature) != 0:
                    AddLinkSignature(typeof(InputLink<T>));
                    break;
                case InitPortStrategy.PortViewGeneration when (portSetup & PortSetup.Editor) != 0:
                    // Editor port view
                    portView = CreateInstance<PortView>();
                    AddPortView(portView, typeof(T).FullName, capacity, graph.InputPortDirection, portName);
                    break;
                case InitPortStrategy.LinkGeneration when (portSetup & PortSetup.Runtime) != 0:
                    // Actual node port
                    InputMultiLink<T> nodePort = new InputMultiLink<T>();
                    AddLink<T>(nodePort, capacity, graph.InputPortDirection);
                    return nodePort;
            }
            return null;
        }

        protected InputLink<T> AddInputLink<T>(string portName = "default",
            PortSetup portSetup = PortSetup.Editor | PortSetup.Signature | PortSetup.Runtime,
            InputPortSettings inputPortSettings = InputPortSettings.None)
        {
            Port.Capacity capacity = Port.Capacity.Single;
            PortView portView;
            switch (currentStrategy)
            {
                case InitPortStrategy.None when (portSetup & PortSetup.Editor) != 0:
                    // Editor port view
                    portView = CreateInstance<PortView>();
                    AddPortView(portView, typeof(T).FullName, capacity, graph.InputPortDirection, portName);
                    break;
                case InitPortStrategy.None:
                    throw new Exception("Cannot setup ports with None strategy!");
                case InitPortStrategy.StringGeneration when (portSetup & PortSetup.Signature) != 0:
                    AddLinkSignature(typeof(InputLink<T>));
                    break;
                case InitPortStrategy.PortViewGeneration when (portSetup & PortSetup.Editor) != 0:
                    // Editor port view
                    portView = CreateInstance<PortView>();
                    AddPortView(portView, typeof(T).FullName, capacity, graph.InputPortDirection, portName);
                    break;
                case InitPortStrategy.LinkGeneration when (portSetup & PortSetup.Runtime) != 0:
                    // Actual node port
                    InputLink<T> link = null;
                    if (CacheOutput)
                    {
                        link = new InputLinkCached<T>();
                        cacheLinks.Add(link);
                    }
                    else
                    {
                        link = new InputLink<T>();
                    }
                    if ((inputPortSettings | InputPortSettings.Optional) != 0)
                        link.optional = true;
                    AddLink<T>(link, capacity, graph.InputPortDirection);
                    return link;
            }
            return null;
        }

        protected OutputLink<T> AddOutputLink<T>(OutputFunction<T> function, Port.Capacity capacity = Port.Capacity.Multi, string portName = "default",
            PortSetup portSetup = PortSetup.Editor | PortSetup.Signature | PortSetup.Runtime)
        {
            Direction direction = graph.InputPortDirection == Direction.Input ? Direction.Output : Direction.Input;
            PortView portView;

            switch (currentStrategy)
            {
                case InitPortStrategy.None when (portSetup & PortSetup.Editor) != 0:
                    // Editor port view
                    portView = CreateInstance<PortView>();
                    AddPortView(portView, typeof(T).FullName, capacity, direction, portName);
                    break;
                case InitPortStrategy.None:
                    throw new Exception("Cannot setup ports with None strategy!");
                case InitPortStrategy.StringGeneration when (portSetup & PortSetup.Signature) != 0:
                    AddLinkSignature(typeof(OutputLink<T>));
                    break;
                case InitPortStrategy.PortViewGeneration when (portSetup & PortSetup.Editor) != 0:
                    // Editor port view
                    portView = CreateInstance<PortView>();
                    AddPortView(portView, typeof(T).FullName, capacity, direction, portName);
                    break;
                case InitPortStrategy.LinkGeneration when (portSetup & PortSetup.Runtime) != 0:
                    // Actual node link
                    OutputLink<T> link = null;
                    if (CacheOutput)
                    {
                        link = new OutputLinkCached<T>();
                        cacheLinks.Add(link);
                    }
                    else
                    {
                        link = new OutputLink<T>();
                    }
                    AddLink<T>(link, capacity, graph.InputPortDirection);
                    link.SubscribePort(function);
                    return link;
                    break;
            }
            return null;
        }

        protected Link AddPropertyOutputLink(string propertyName)
        {
            Direction direction = graph.InputPortDirection == Direction.Input ? Direction.Output : Direction.Input;
            PortView portView;

            switch (currentStrategy)
            {
                case InitPortStrategy.StringGeneration:
                    AddLinkSignature("PropertyOutputLink<" + propertyName + ":" + graph.GetPropertyType(propertyName) + ">");
                    break;
                case InitPortStrategy.PortViewGeneration:
                    portView = CreateInstance<PortView>();
                    AddPortView(portView, graph.GetPropertyType(propertyName), Port.Capacity.Multi, direction, propertyName);
                    break;
                case InitPortStrategy.LinkGeneration:
                    Link link = graph.GetProperty(propertyName);
                    links.Add(link);
                    return link;
                default:
                    break;

            }
            return null;
        }

        protected InputLink<T> AddPropertyInputLink<T>(string propertyName)
        {
            PortView portView;

            switch (currentStrategy)
            {
                case InitPortStrategy.StringGeneration:
                    AddLinkSignature("PropertyInputLink<" + propertyName + ":" + graph.GetPropertyType(propertyName) + ">");
                    break;
                case InitPortStrategy.PortViewGeneration:

                    break;
                case InitPortStrategy.LinkGeneration:
                    Link propertyLink = graph.GetProperty(propertyName);
                    InputLink<T> link = null;
                    if (CacheOutput)
                    {
                        link = new InputLinkCached<T>();
                        cacheLinks.Add(link);
                    }
                    else
                    {
                        link = new InputLink<T>();
                    }
                    link.Init(this, links.Count, graph.Orientation, Direction.None, Port.Capacity.Single);
                    link.Valid = true;
                    link.LinkPort(propertyLink);
                    return link;
                default:
                    break;

            }
            return null;
        }

        public void AddLinkSignature(Type type)
        {
            if (currentStrategy != InitPortStrategy.StringGeneration)
                return;

            portSignature += type.FullName;
        }

        public void AddLinkSignature(string name)
        {
            if (currentStrategy != InitPortStrategy.StringGeneration)
                return;

            portSignature += name;
        }

        private void AddLink<T>(Link link, Port.Capacity capacity, Direction direction)
        {
            link.Init(this, links.Count, graph.Orientation, direction, capacity);
            links.Add(link);
            link.Valid = true;
        }

        private void AddPortView(PortView portView, string type, Port.Capacity capacity, Direction direction, string portName)
        {
            portView.Init(this, graph.Orientation, direction, capacity, type, portViews.Count, portName);
            portView.Valid = true;
            if (!Application.isPlaying)
            {
                EditorUtility.SetDirty(portView);
                AssetDatabase.AddObjectToAsset(portView, graph);
            }
            portViews.Add(portView);
            if (!Application.isPlaying)
            {
                AssetDatabase.SaveAssets();
            }
        }

        public void ConnectLinks()
        {
            if (links == null || portViews == null || links.Count != portViews.Count)
                throw new Exception("Wrong ports setup.");

            for (int i = 0; i < links.Count; i++)
            {
                ILinkable inputlink = links[i] as ILinkable;
                if (inputlink != null && portViews[i].linked.Count > 0)
                {
                    foreach (PortView otherPortView in portViews[i].linked)
                    {
                        int otherIndex = otherPortView.index;
                        Link outputPort = otherPortView.node.links[otherIndex];
                        inputlink.LinkPort(outputPort);
                    }
                }
            }
        }

        public bool ReEvaluate()
        {
            bool retval = false;
            foreach (Link l in cacheLinks)
            {
                ICachedLink cl = l as ICachedLink;
                if (cl != null)
                {
                    if (cl.ReEvaluate())
                    {
                        retval = true;
                    }
                }
            }
            return retval;
        }

        public virtual Node Clone()
        {
            Node retval = CreateInstance(this.GetType()) as Node;
            retval.nodePosition = this.nodePosition;

            return retval;
        }
    }
}