using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.GraphCreator
{
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
    }
}