using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.GraphCreator
{
    public class InputMultiLink<T> : Link, ILinkable
    {
        public List<IOutputLink<T>> outputLinks = new List<IOutputLink<T>>();
        private Type type;

        public ILink LinkedPort => outputLinks[0];

        public IOutputLink<T> this[int index]
        {
            get { return outputLinks[index]; }
        }

        public override Type PortType => type;

        public override bool IsInput => true;

        public override void Init(Node node, int index, Orientation rotientation, Direction direction, Capacity capacity)
        {
            this._orientation = orientation;
            this._direction = Direction.Input;
            this._capacity = capacity;
            type = typeof(T);
            this.node = node;
            this.nodeIndex = index;
        }

        public void LinkPort(Link outputPort)
        {
            outputLinks.Add((IOutputLink<T>)outputPort);
        }

        public void RemoveLink()
        {
            outputLinks.Clear();
        }

        public T[] EvaluateAll()
        {
            T[] retval = new T[outputLinks.Count];

            for (int i = 0; i < outputLinks.Count; i++)
            {
                retval[i] = outputLinks[i].Evaluate();
            }

            return retval;
        }

        public override Link Clone(Node node)
        {
            throw new NotImplementedException();
        }
    }
}