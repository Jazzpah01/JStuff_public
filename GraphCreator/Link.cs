using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEditor.Experimental.GraphView;

namespace JStuff.GraphCreator
{
    public abstract class Link : IInvalid, ICollapsable
    {
        public enum Orientation
        {
            Horizontal,
            Vertical
        }

        public enum Direction
        {
            None,
            Input,
            Output
        }

        public enum Capacity
        {
            Single,
            Multi
        }

        public Node node;
        public int nodeIndex;
        public int graphIndex;
        protected Orientation _orientation;
        protected Direction _direction;
        protected Capacity _capacity;

        private bool valid = false;

        public Orientation orientation => _orientation;
        public Direction direction => _direction;
        public Capacity capacity => _capacity;

        public abstract bool IsInput { get; }

        public abstract Type PortType { get; }
        public bool Valid { get => valid; set => valid = true; }

        public abstract Link Clone(Node node);
        public abstract void Init(Node node, int index, Orientation rotientation, Direction direction, Capacity capacity);

        public virtual void Collapse()
        {
            
        }

        public virtual bool IsConstant()
        {
            return false;
        }
    }
}