using JStuff.GraphCreator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.GraphCreator
{
    [CreateNodePath("Math/Vector/Dot")]
    public class DotNode : VectorMath
    {
        InputLink<Vector2> vector1;
        InputLink<Vector2> vector2;
        OutputLink<float> output;

        protected override void SetupPorts()
        {
            vector1 = AddInputLink<Vector2>(portName: "Vector2");
            vector2 = AddInputLink<Vector2>(portName: "Vector2");
            output = AddOutputLink<float>(Evaluate);
        }

        public float Evaluate()
        {
            return Vector2.Dot(vector1.Evaluate(), vector2.Evaluate());
        }
    }
}