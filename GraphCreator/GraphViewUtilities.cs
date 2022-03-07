using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gv = UnityEditor.Experimental.GraphView;

namespace JStuff.GraphCreator
{
    public static class GraphCreatorUtilities
    {
        public static gv.Direction Get(this Direction direction)
        {
            return direction == Direction.Input ? gv.Direction.Input : gv.Direction.Output;
        }

        public static Direction Get(this gv.Direction direction)
        {
            return direction == gv.Direction.Input ? Direction.Input : Direction.Output;
        }
    }
}