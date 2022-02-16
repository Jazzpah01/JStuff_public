using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
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
            outputLinks.Add((IOutputLink<T>)outputLinks);
        }

        public void RemoveLink()
        {
            throw new NotImplementedException();
        }

        public override Link Clone(Node node)
        {
            throw new NotImplementedException();
        }
    }
}