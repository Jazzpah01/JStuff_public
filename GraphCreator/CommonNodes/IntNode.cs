using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.GraphCreator
{
    [CreateNodePath("Common/Value/int")]
    public class IntNode : CommonNode
    {
        public int value;

        OutputLink<int> link;

        protected override void SetupPorts()
        {
            link = AddOutputLink<int>(Evaluate);
        }

        private int Evaluate()
        {
            return value;
        }

        public override Node Clone()
        {
            IntNode node = base.Clone() as IntNode;

            node.value = value;

            return node;
        }
    }
}