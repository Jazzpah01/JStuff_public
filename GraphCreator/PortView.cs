using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.GraphView;
#endif
using UnityEngine;

namespace JStuff.GraphCreator
{
    [Serializable]
    public class PortView : ScriptableObject, IInvalid
    {
        public Node node;
        public Link.Orientation orientation;
        public Link.Direction direction;
        public Link.Capacity capacity;
        [SerializeField] private string portType;
        public int index;
        public int graphIndex;
        public int UIIndex;
        public string portName;
        [SerializeField] private bool isInput;

        public List<PortView> linked = new List<PortView>();

        //[SerializeField] private
        public bool isValid = false;

        public Type PortType => Type.GetType(portType);

        public string PortTypeName => portType;

        public bool Valid { get => isValid; set => isValid = value; }

        public void Init(Node node, Link.Orientation orientation, Link.Direction direction, Link.Capacity capacity, string portTypeName, int index, string portName = "default")
        {
            name = $"{direction} Port: {portTypeName}";
            this.node = node;
            this.orientation = orientation;
            this.direction = direction;
            this.capacity = capacity;
            portType = portTypeName;
            this.index = index;
            isInput = direction == Link.Direction.Input;
            if (portName == "default")
            {
                Type t = Type.GetType(portType);
                if (t!= null)
                    this.portName = Type.GetType(portType).Name;
            } else
            {
                this.portName = portName;
            }

#if UNITY_EDITOR
            AssetDatabase.SaveAssets();
#endif
        }

        public void ConnectPort(PortView view)
        {
            if (view.isInput)
                throw new Exception("Cannot connect to an input PortView.");
            if (!isInput)
                throw new Exception("Cannot connect from an output PortView.");
            if (capacity == Link.Capacity.Single && linked.Count > 0)
                throw new Exception("Cannot connect multiple to a single capacity input PortView.");

            linked.Add(view);
#if UNITY_EDITOR
            AssetDatabase.SaveAssets();
#endif
        }

        public void UnLink(PortView view)
        {
            linked.Remove(view);
#if UNITY_EDITOR
            AssetDatabase.SaveAssets();
#endif
        }

        public void UnLinkAll()
        {
            linked = new List<PortView>();
#if UNITY_EDITOR
            AssetDatabase.SaveAssets();
#endif
        }

        public bool LinkedWith(PortView view)
        {
            return linked.Contains(view);
        }
    }
}