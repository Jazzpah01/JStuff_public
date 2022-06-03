using JStuff.GraphCreator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.GraphCreator
{
    [CreateNodePath("Math/Vector/Split")]
    public class VectorSplit : CommonNode
    {
        InputLink<Vector2> input;
        OutputLink<float> outputX;
        OutputLink<float> outputY;

        protected override void SetupPorts()
        {
            input = AddInputLink<Vector2>(portName: "Vector2");
            outputX = AddOutputLink<float>(EvaluateX);
            outputY = AddOutputLink<float>(EvaluateY);
        }

        private float EvaluateX()
        {
            return input.Evaluate().x;
        }
        private float EvaluateY()
        {
            return input.Evaluate().y;
        }

        public override Node Clone()
        {
            VectorSplit node = base.Clone() as VectorSplit;

            return node;
        }
    }
}