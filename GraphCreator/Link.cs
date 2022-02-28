using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace JStuff.GraphCreator
{
    public abstract class Link : IInvalid
    {
        public Node node;
        public int nodeIndex;
        public int graphIndex;
        protected Orientation orientation;
        protected Direction direction;
        protected Port.Capacity capacity;

        private bool valid = false;

        public Orientation Orientation => orientation;
        public Direction Direction => direction;
        public Port.Capacity Capacity => capacity;

        public abstract Type PortType { get; }
        public bool Valid { get => valid; set => valid = true; }

        public abstract Link Clone(Node node);
        public abstract void Init(Node node, int index, Orientation rotientation, Direction direction, Port.Capacity capacity);
    }
}