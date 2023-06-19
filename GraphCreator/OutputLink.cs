using System;
using System.Collections;
using System.Collections.Generic;
//using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace JStuff.GraphCreator
{
    [Serializable]
    public class OutputLink<T> : Link, IOutputLink<T>, IObserverPort<T>
    {
        public OutputFunction<T> function;
        Type type;
        public override Type PortType => type;

        public override bool IsInput => false;

        public override void Init(Node node, int index, Orientation rotientation, Direction direction, Capacity capacity)
        {
            this._orientation = orientation;
            this._direction = Direction.Output;
            this._capacity = capacity;
            type = typeof(T);
            this.node = node;
            this.nodeIndex = index;
            if (node == null)
                Debug.Log("Node is null! :(");
        }

        public void SubscribePort(OutputFunction<T> function)
        {
            this.function = function;
        }

        public virtual T Evaluate()
        {
            return function();
        }

        public override Link Clone(Node node)
        {
            //SimpleNodeOutputPort<T> retval = new SimpleNodeOutputPort<T>();
            //retval.Init(node.Clone(), index, this.Orientation, this.Direction, this.Capacity);
            //return retval;
            throw new Exception("Method not implemented exception.");
        }

        public override bool IsConstant()
        {
            return node.IsConstant();
        }
    }
}