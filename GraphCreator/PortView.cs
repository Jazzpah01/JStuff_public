using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace JStuff.GraphCreator
{
    [Serializable]
    public class PortView : ScriptableObject, IInvalid
    {
        public Node node;
        public Orientation orientation;
        public Direction direction;
        public Port.Capacity capacity;
        [SerializeField] private string portType;
        public int index;
        public int graphIndex;
        public int UIIndex;
        public string portName;
        [SerializeField] private bool isInput;

        public List<PortView> linked = new List<PortView>();

        [SerializeField] private bool isValid = false;

        public Type PortType => Type.GetType(portType);

        public bool Valid { get => isValid; set => isValid = value; }

        public void Init(Node node, Orientation orientation, Direction direction, Port.Capacity capacity, string portTypeName, int index, string portName = "default")
        {
            name = $"{direction} Port: {portTypeName}";
            this.node = node;
            this.orientation = orientation;
            this.direction = direction;
            this.capacity = capacity;
            portType = portTypeName;
            this.index = index;
            isInput = direction == Direction.Input;
            if (portName == "default")
            {
                Type t = Type.GetType(portType);
                if (t!= null)
                    this.portName = Type.GetType(portType).Name;
            } else
            {
                this.portName = portName;
            }
                
            AssetDatabase.SaveAssets();
        }

        public void ConnectPort(PortView view)
        {
            if (view.isInput)
                throw new Exception("Cannot connect to an input PortView.");
            if (!isInput)
                throw new Exception("Cannot connect from an output PortView.");
            if (capacity == Port.Capacity.Single && linked.Count > 0)
                throw new Exception("Cannot connect multiple to a single capacity input PortView.");

            linked.Add(view);
            AssetDatabase.SaveAssets();
        }

        public void UnLink(PortView view)
        {
            linked.Remove(view);
            AssetDatabase.SaveAssets();
        }

        public void UnLinkAll()
        {
            linked = new List<PortView>();
            AssetDatabase.SaveAssets();
        }

        public bool LinkedWith(PortView view)
        {
            return linked.Contains(view);
        }
    }
}