using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.GraphCreator
{
    [CreateNodePath("Math/Single/Divide")]
    public class DivideNode : SingleMath
    {
        InputLink<float> v1;
        InputLink<float> v2;
        OutputLink<float> output;

        protected override void SetupPorts()
        {
            v1 = AddInputLink<float>();
            v2 = AddInputLink<float>();
            output = AddOutputLink(Evaluate);
        }

        public float Evaluate()
        {
            return v1.Evaluate() * v2.Evaluate();
        }
    }
}