using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.GraphCreator
{
    public class CollapsedLink<T> : OutputLink<T>
    {
        public T value;
        //public LinkedPort;

        public CollapsedLink(T value)
        {
            this.value = value;
        }

        public override Type PortType => typeof(T);

        public ILink LinkedPort => throw new NotImplementedException();

        public override Link Clone(Node node)
        {
            throw new Exception("Method not implemented exception.");
        }

        public override void Init(Node node, int index, Orientation rotientation, Direction direction, Capacity capacity)
        {
            throw new NotImplementedException();
        }

        public override T Evaluate()
        {
            return value;
        }

        public override bool IsConstant()
        {
            return true;
        }
    }
}