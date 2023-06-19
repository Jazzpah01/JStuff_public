using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.GraphCreator
{
    public class OutputLinkCached<T> : OutputLink<T>
    {
        public T cachedValue;
        public int iteration = 0;

        public override T Evaluate()
        {
            if (iteration == node.graph.rootNode.iteration && iteration > 0)
            {
                return cachedValue;
            }
            else if (iteration < node.iteration || node.ReEvaluate())
            {
                iteration = node.graph.rootNode.iteration;
                node.iteration = iteration;
                cachedValue = base.Evaluate();
                return cachedValue;
            } else
            {
                return cachedValue;
            }
        }
    }
}