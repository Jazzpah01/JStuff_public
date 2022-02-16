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
        [HideInInspector] public bool initialized = false;

        [HideInInspector] public Vector2 nodePosition;
        [HideInInspector] private List<Link> ports = new List<Link>();
        [HideInInspector] [SerializeReference] public List<PortView> portViews = new List<PortView>();
        [HideInInspector] public string guid;
        [HideInInspector] public Graph graph;

        public virtual StyleSheet StyleSheet => null;

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
        public enum PortSetup
        {
            None = 0,
            Editor = 0b1,
            Signature = 0b10,
            Runtime = 0b100
        }

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
        public List<Link> Ports => ports;

        public bool Valid { get => valid; set => valid = value; }

        [SerializeField] private string portSignature = "";

        private InitPortStrategy currentStrategy = InitPortStrategy.None;

        private enum InitPortStrategy
        {
            None,
            StringGeneration,
            PortViewGeneration,
            PortGeneration
        }

        public void InitializeNode()
        {
            if (!initialized)
            {
                Initialize();
                initialized = true;
            }

            // Application.isPlaying, then do that
            if (Application.isPlaying)
            {
                SetupPorts_Editor(InitPortStrategy.PortGeneration);
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

                portViews = new List<PortView>();
                SetupPorts_Editor(InitPortStrategy.PortViewGeneration);

                if (OnNodeChange != null)
                    OnNodeChange();
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

        protected InputMultiLink<T> AddInputMultiLink<T>(PortSetup portSetup = PortSetup.Editor | PortSetup.Signature | PortSetup.Runtime)
        {
            Port.Capacity capacity = Port.Capacity.Multi;
            PortView portView;
            switch (currentStrategy)
            {
                case InitPortStrategy.None when (portSetup & PortSetup.Editor) != 0:
                    // Editor port view
                    portView = CreateInstance<PortView>();
                    AddPortView(portView, typeof(T).FullName, capacity, graph.InputPortDirection);
                    break;
                case InitPortStrategy.None:
                    throw new Exception("Cannot setup ports with None strategy!");
                case InitPortStrategy.StringGeneration when (portSetup & PortSetup.Signature) != 0:
                    AddLinkSignature(typeof(InputLink<T>));
                    break;
                case InitPortStrategy.PortViewGeneration when (portSetup & PortSetup.Editor) != 0:
                    // Editor port view
                    portView = CreateInstance<PortView>();
                    AddPortView(portView, typeof(T).FullName, capacity, graph.InputPortDirection);
                    break;
                case InitPortStrategy.PortGeneration when (portSetup & PortSetup.Runtime) != 0:
                    // Actual node port
                    InputMultiLink<T> nodePort = new InputMultiLink<T>();
                    AddLink<T>(nodePort, capacity, graph.InputPortDirection);
                    return nodePort;
            }
            return null;
        }

        protected InputLink<T> AddInputLink<T>(PortSetup portSetup = PortSetup.Editor | PortSetup.Signature | PortSetup.Runtime)
        {
            Port.Capacity capacity = Port.Capacity.Single;
            PortView portView;
            switch (currentStrategy)
            {
                case InitPortStrategy.None when (portSetup & PortSetup.Editor) != 0:
                    // Editor port view
                    portView = CreateInstance<PortView>();
                    AddPortView(portView, typeof(T).FullName, capacity, graph.InputPortDirection);
                    break;
                case InitPortStrategy.None:
                    throw new Exception("Cannot setup ports with None strategy!");
                case InitPortStrategy.StringGeneration when (portSetup & PortSetup.Signature) != 0:
                    AddLinkSignature(typeof(InputLink<T>));
                    break;
                case InitPortStrategy.PortViewGeneration when (portSetup & PortSetup.Editor) != 0:
                    // Editor port view
                    portView = CreateInstance<PortView>();
                    AddPortView(portView, typeof(T).FullName, capacity, graph.InputPortDirection);
                    break;
                case InitPortStrategy.PortGeneration when (portSetup & PortSetup.Runtime) != 0:
                    // Actual node port
                    InputLink<T> nodePort = new InputLink<T>();
                    AddLink<T>(nodePort, capacity, graph.InputPortDirection);
                    return nodePort;
            }
            return null;
        }

        protected OutputLink<T> AddOutputLink<T>(OutputFunction<T> function, Port.Capacity capacity = Port.Capacity.Multi,
            PortSetup portSetup = PortSetup.Editor | PortSetup.Signature | PortSetup.Runtime)
        {
            Direction direction = graph.InputPortDirection == Direction.Input ? Direction.Output : Direction.Input;
            PortView portView;

            switch (currentStrategy)
            {
                case InitPortStrategy.None when (portSetup & PortSetup.Editor) != 0:
                    // Editor port view
                    portView = CreateInstance<PortView>();
                    AddPortView(portView, typeof(T).FullName, capacity, direction);
                    break;
                case InitPortStrategy.None:
                    throw new Exception("Cannot setup ports with None strategy!");
                case InitPortStrategy.StringGeneration when (portSetup & PortSetup.Signature) != 0:
                    AddLinkSignature(typeof(OutputLink<T>));
                    break;
                case InitPortStrategy.PortViewGeneration when (portSetup & PortSetup.Editor) != 0:
                    // Editor port view
                    portView = CreateInstance<PortView>();
                    AddPortView(portView, typeof(T).FullName, capacity, direction);
                    break;
                case InitPortStrategy.PortGeneration when (portSetup & PortSetup.Runtime) != 0:
                    // Actual node port
                    OutputLink<T> nodePort = new OutputLink<T>();
                    AddLink<T>(nodePort, capacity, graph.InputPortDirection);
                    nodePort.SubscribePort(function);
                    return nodePort;
                    break;
            }
            return null;
        }

        protected Link AddPropertyLink(string propertyName)
        {
            Direction direction = graph.InputPortDirection == Direction.Input ? Direction.Output : Direction.Input;
            PortView portView;

            Debug.Log("halllooo");

            switch (currentStrategy)
            {
                case InitPortStrategy.StringGeneration:
                    AddLinkSignature("PropertyLink<" + graph.GetPropertyType(propertyName) + ">");
                    break;
                case InitPortStrategy.PortViewGeneration:
                    portView = CreateInstance<PortView>();
                    AddPortView(portView, graph.GetPropertyType(propertyName), Port.Capacity.Multi, direction);
                    Debug.Log("Generation port view: " + Time.time);
                    break;
                case InitPortStrategy.PortGeneration:
                    Link link = graph.GetProperty(propertyName);
                    ports.Add(link);
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

        private void AddLink<T>(Link nodePort, Port.Capacity capacity, Direction direction)
        {
            nodePort.Init(this, ports.Count, graph.Orientation, direction, capacity);
            ports.Add(nodePort);
            nodePort.Valid = true;
        }

        private void AddPortView(PortView portView, string type, Port.Capacity capacity, Direction direction)
        {
            portView.Init(this, graph.Orientation, direction, capacity, type, portViews.Count);
            portView.Valid = true;
            EditorUtility.SetDirty(portView);
            AssetDatabase.AddObjectToAsset(portView, graph);
            portViews.Add(portView);
            AssetDatabase.SaveAssets();
        }

        public void LinkNodePorts()
        {
            if (ports == null || portViews == null || ports.Count != portViews.Count)
                throw new Exception("Wrong ports setup.");

            for (int i = 0; i < ports.Count; i++)
            {
                ILinkable inputport = ports[i] as ILinkable;
                if (inputport != null && portViews[i].linked.Count > 0)
                {
                    foreach (PortView portView in portViews[i].linked)
                    {
                        int otherIndex = portView.index;
                        Link outputPort = portView.node.ports[otherIndex];
                        inputport.LinkPort(outputPort);
                    }
                }
            }
        }

        public virtual Node Clone()
        {
            Node node = Instantiate(this);
            return node;
        }
    }
}