using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace JStuff.GraphCreator
{
    [Serializable]
    public class InputLink<T> : Link, ILinkable
    {
        public IOutputLink<T> linkedPort;
        public IOutputLink<T> LinkedPort => linkedPort;
        Type type;
        public override Type PortType => type;

        ILink ILinkable.LinkedPort => linkedPort;

        public override void Init(Node node, int index, Orientation rotientation, Direction direction, Port.Capacity capacity)
        {
            orientation = orientation;
            this.direction = Direction.Input;
            this.capacity = capacity;
            type = typeof(T);
            this.node = node;
            this.index = index;
        }

        public void LinkPort(Link outputPort)
        {
            IOutputLink<T> realPort = (IOutputLink<T>)outputPort;
            linkedPort = realPort;
        }

        public void RemoveLink()
        {
            linkedPort = null;
        }

        public T Evaluate()
        {
            return linkedPort.Evaluate();
        }

        public override Link Clone(Node node)
        {
            throw new Exception("Method not implemented exception.");
        }
    }
}