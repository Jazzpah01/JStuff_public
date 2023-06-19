using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.GraphCreator
{
    [System.AttributeUsage(System.AttributeTargets.Class)
    ]
    public class CreateNodePath : System.Attribute
    {
        public string path;

        public CreateNodePath(string path)
        {
            this.path = path;
        }
    }
}