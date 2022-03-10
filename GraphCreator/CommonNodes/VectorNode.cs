using JStuff.GraphCreator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.GraphCreator
{
    public class VectorNode : CommonNode
    {
        public Vector2 value;

        OutputLink<Vector2> link;

        protected override void SetupPorts()
        {
            link = AddOutputLink<Vector2>(Evaluate, portName: "Vector2");
        }

        private Vector2 Evaluate()
        {
            return value;
        }

        public override Node Clone()
        {
            VectorNode node = base.Clone() as VectorNode;

            node.value = value;

            return node;
        }
    }
}