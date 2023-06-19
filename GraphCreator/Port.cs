using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.GraphView;
#endif
using UnityEngine;
using System.Linq;

namespace JStuff.GraphCreator
{
    [Serializable]
    public class Port : IInvalid
    {
        public string name;
        public Node node;
        public Link.Orientation orientation;
        public Link.Direction direction;
        public Link.Capacity capacity;
        public string portType;
        public int nodeIndex;
        public int graphIndex;
        public int UIIndex;
        public string portName;
        public bool isInput;

        public List<Node> connectedNodes = new List<Node>();
        public List<int> connectedPortIndex = new List<int>();

        public bool isValid = false;

        public Type PortType => Type.GetType(portType);

        public string PortTypeName => portType;

        public bool Valid { 
            get { 
                if (connectedNodes == null || connectedPortIndex == null || 
                    connectedNodes.Count != connectedPortIndex.Count)
                {
                    isValid = false;
                }
                return isValid;
            }
            set => isValid = value; 
        }

        public void Init(Node node, Link.Orientation orientation, Link.Direction direction, Link.Capacity capacity, string portTypeName, int index, string portName = "default")
        {
            name = $"{direction} Port: {portTypeName}";
            this.node = node;
            this.orientation = orientation;
            this.direction = direction;
            this.capacity = capacity;
            portType = portTypeName;
            this.nodeIndex = index;
            isInput = direction == Link.Direction.Input;
            this.isValid = true;
            if (portName == "default")
            {
                Type t = Type.GetType(portType);
                if (t != null)
                    this.portName = Type.GetType(portType).Name;
            }
            else
            {
                this.portName = portName;
            }
        }

        public void ConnectPort(Port port)
        {
            if (port.isInput)
                throw new Exception("Cannot connect to an input PortView.");
            if (!isInput)
                throw new Exception("Cannot connect from an output PortView.");
            if (capacity == Link.Capacity.Single && connectedNodes.Count > 0)
                throw new Exception("Cannot connect multiple to a single capacity input PortView.");

            connectedNodes.Add(port.node);
            connectedPortIndex.Add(port.nodeIndex);
        }

        public void UnLink(Port port)
        {
            int i = connectedNodes.IndexOf(port.node);
            connectedNodes.RemoveAt(i);
            connectedPortIndex.RemoveAt(i);
        }

        public void UnLinkAll()
        {
            connectedNodes.Clear();
            connectedPortIndex.Clear();
        }

        public bool LinkedWith(Port port)
        {
            for (int i = 0; i < connectedNodes.Count; i++)
            {
                if (connectedNodes[i] == port.node && connectedPortIndex[i] == port.nodeIndex)
                    return true;
            }

            return false;
        }

        public void Destroy()
        {
            name = null;
            node = null;
            portType = null;
            nodeIndex = -1;
            graphIndex = -1;
            UIIndex = -1;
            portName = null;
            isInput = false;
            connectedNodes = null;
            isValid = false;
        }

        public Port[] GetConnectedPorts()
        {
            if (connectedNodes == null)
                return new Port[0];

            Port[] retval = new Port[connectedNodes.Count];

            for (int i = 0; i < connectedNodes.Count; i++)
            {
                if (connectedNodes[i] == null || connectedNodes[i].Length == 0)
                    return null;

                retval[i] = connectedNodes[i]?.ports[connectedPortIndex[i]];

                if (retval[i] == null)
                {
                    return null;
                }
            }

            var sorted = from Port in retval
                         orderby Port.node.nodePosition.y
                         select Port;

            return sorted.ToArray();
        }

        public override string ToString()
        {
            string connections = "";

            for (int i = 0; i < connectedNodes.Count; i++)
            {
                connections += connectedNodes[i].name + ", ";
            }

            return $"{name}({connections})";
        }
    }

    
}