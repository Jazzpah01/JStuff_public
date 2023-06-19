using JStuff.GraphCreator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.GraphCreator
{
    public class InputLinkCached<T> : InputLink<T>, ICachedLink
    {
        public T cachedValue;
        public int iteration = 0;
        public bool it = false;

        public override T Evaluate()
        {
            return cachedValue;
        }

        public bool ReEvaluate()
        {
            if (connectedLink == null && optional)
                return false;

            T val = base.Evaluate();
            if (EqualityComparer<T>.Default.Equals(cachedValue, val))
            {
                cachedValue = val;
                return false;
            } else
            {
                cachedValue = val;
                return true;
            }
        }
    }
}