using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.GraphCreator
{
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
    }
}