using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.GraphCreator
{
    [Serializable]
    public class InputLink<T> : Link, ILinkable
    {
        public bool optional = false;
        public IOutputLink<T> linkedPort;
        public IOutputLink<T> LinkedPort => linkedPort;
        Type type;
        public override Type PortType => type;

        ILink ILinkable.LinkedPort => linkedPort;

        public override void Init(Node node, int index, Link.Orientation rotientation, Link.Direction direction, Link.Capacity capacity)
        {
            this._orientation = orientation;
            this._direction = Direction.Input;
            this._capacity = capacity;
            type = typeof(T);
            this.node = node;
            this.nodeIndex = index;
        }

        public void LinkPort(Link outputLink)
        {
            IOutputLink<T> realLink = (IOutputLink<T>)outputLink;
            linkedPort = realLink;
        }

        public void RemoveLink()
        {
            linkedPort = null;
        }

        public virtual T Evaluate()
        {
            return linkedPort.Evaluate();
        }

        public override Link Clone(Node node)
        {
            throw new Exception("Method not implemented exception.");
        }
    }
}