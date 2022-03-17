using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.GraphCreator
{
    [CreateNodePath("Common/Value/float")]
    public class FloatNode : CommonNode
    {
        public float value;

        OutputLink<float> link;

        protected override void SetupPorts()
        {
            link = AddOutputLink<float>(Evaluate);
        }

        private float Evaluate()
        {
            return value;
        }
        public override Node Clone()
        {
            FloatNode node = base.Clone() as FloatNode;

            node.value = value;

            return node;
        }

    }
}