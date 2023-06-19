using System;
using System.Collections;
using System.Collections.Generic;
//using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace JStuff.GraphCreator
{
    public class PropertyLink<T> : Link, IOutputLink<T>, ICollapsable
    {
        public Graph graph;
        public Func<object> function;
        public T cachedValue;
        public Type type;
        public bool isConstant = false;

        public override Type PortType => throw new NotImplementedException();

        public override Link Clone(Node node)
        {
            throw new NotImplementedException();
        }

        public T Value
        {
            set
            {
                if (isConstant)
                {
                    throw new Exception("Cannot change a constant property at runtime.");
                }

                cachedValue = value;
            }
        }

        public override bool IsInput => false;

        public T Evaluate()
        {
            return cachedValue;
        }

        public override void Init(Node node, int index, Orientation rotientation, Direction direction, Capacity capacity)
        {
            this._orientation = rotientation;
            this._direction = Direction.Output;
            this._capacity = capacity;
            type = typeof(T);
            this.node = node;
            this.nodeIndex = index;
        }

        public override bool IsConstant()
        {
            return isConstant;
        }
    }
}