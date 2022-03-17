using JStuff.GraphCreator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.GraphCreator
{
    [CreateNodePath("Math/Vector/Divide")]
    public class VectorDivide : VectorMath
    {
        InputLink<Vector2> vector;
        InputLink<float> devisor;
        OutputLink<Vector2> output;

        protected override void SetupPorts()
        {
            vector = AddInputLink<Vector2>(portName: "Vector2");
            devisor = AddInputLink<float>();
            output = AddOutputLink(Evaluate, portName: "Vector2");
        }

        public Vector2 Evaluate()
        {
            return vector.Evaluate() / devisor.Evaluate();
        }
    }
}