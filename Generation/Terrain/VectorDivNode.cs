using JStuff.GraphCreator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.Generation.Terrain
{
    public class VectorDivNode : Node
    {
        InputLink<Vector2> vector;
        InputLink<float> devisor;
        OutputLink<Vector2> output;

        protected override void SetupPorts()
        {
            vector = AddInputLink<Vector2>();
            devisor = AddInputLink<float>();
            output = AddOutputLink(Evaluate);
        }

        public Vector2 Evaluate()
        {
            return vector.Evaluate() / devisor.Evaluate();
        }
    }
}