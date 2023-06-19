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
        public IOutputLink<T> connectedLink;
        public IOutputLink<T> LinkedPort => connectedLink;
        Type type;
        public override Type PortType => type;

        ILink ILinkable.LinkedPort => connectedLink;

        public override bool IsInput => true;

        public bool collapsed;
        public T CollapsedValue;

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
            connectedLink = realLink;
        }

        public void RemoveLink()
        {
            connectedLink = null;
        }

        public virtual T Evaluate()
        {
            return connectedLink.Evaluate();
        }

        public override Link Clone(Node node)
        {
            throw new Exception("Method not implemented exception.");
        }

        public override void Collapse()
        {
            if (connectedLink == null && optional)
                return;

            T value = connectedLink.Evaluate();
            connectedLink = new CollapsedLink<T>(value);
        }

        public override bool IsConstant()
        {
            ICollapsable c = ((ICollapsable)connectedLink);

            if (c == null)
                return true;

            return c.IsConstant();
        }
    }
}