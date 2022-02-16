using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace JStuff.AI.Flocking
{
    [Serializable]
    public class WeightedBehavior
    {
        [SerializeField] private FlockingBehavior behavior;
        [SerializeField] private float weight;
        [SerializeField] private bool flockAsContext;
        private List<Transform> context;

        public float Weight
        {
            get { return weight; }
        }

        public FlockingBehavior Behavior
        {
            get { return behavior; }
        }

        public bool FlockAsContext
        {
            get { return flockAsContext; }
        }

        public List<Transform> Context
        {
            get { return context; }
        }

        public void SetContext(List<Transform> context)
        {
            this.context = context;
        }

        public void AddContext(Transform t)
        {
            this.context.Add(t);
        }

        public void RemoveContext(Transform t)
        {
            this.context.Remove(t);
        }
    }
}