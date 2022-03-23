using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace JStuff.GraphCreator
{
    public class PropertyLink<T> : Link, IOutputLink<T>
    {
        public Graph graph;
        public Func<object> function;
        public T cachedValue;
        public Type type;

        public override Type PortType => throw new NotImplementedException();

        public override Link Clone(Node node)
        {
            throw new NotImplementedException();
        }

        public T Value
        {
            set
            {
                cachedValue = value;
            }
        }

        public T Evaluate()
        {
            return cachedValue;
        }

        public override void Init(Node node, int index, Orientation rotientation, Direction direction, Port.Capacity capacity)
        {
            orientation = rotientation;
            this.direction = Direction.Output;
            this.capacity = capacity;
            type = typeof(T);
            this.node = node;
            this.nodeIndex = index;
        }
    }
}