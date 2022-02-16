using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.GraphCreator
{
    public class StringNode : CommonNode
    {
        public string value;

        OutputLink<string> link;

        protected override void SetupPorts()
        {
            link = AddOutputLink<string>(Evaluate);
        }

        private string Evaluate()
        {
            return value;
        }
    }
}