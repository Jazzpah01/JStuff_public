using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gv = UnityEditor.Experimental.GraphView;

public static class GraphViewUtilities
{
    public static gv.Direction Get(this Direction direction)
    {
        return (direction == Direction.Input) ? gv.Direction.Input : gv.Direction.Output;
    }

    public static Direction Get(this gv.Direction direction)
    {
        return (direction == gv.Direction.Input) ? Direction.Input : Direction.Output;
    }
}