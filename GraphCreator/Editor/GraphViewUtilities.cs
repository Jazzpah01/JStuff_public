using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gv = UnityEditor.Experimental.GraphView;

namespace JStuff.GraphCreator.Editor
{
    public static class GraphCreatorUtilities
    {
        public static gv.Direction Get(this Link.Direction direction)
        {
            return direction == Link.Direction.Input ? gv.Direction.Input : gv.Direction.Output;
        }

        public static Link.Direction Get(this gv.Direction direction)
        {
            return direction == gv.Direction.Input ? Link.Direction.Input : Link.Direction.Output;
        }

        public static gv.Orientation Get(this Link.Orientation direction)
        {
            return direction == Link.Orientation.Horizontal ? gv.Orientation.Horizontal : gv.Orientation.Vertical;
        }

        public static Link.Orientation Get(this gv.Orientation orientation)
        {
            return orientation == gv.Orientation.Horizontal ? Link.Orientation.Horizontal : Link.Orientation.Vertical;
        }

        public static gv.Port.Capacity Get(this Link.Capacity direction)
        {
            return direction == Link.Capacity.Single ? gv.Port.Capacity.Single : gv.Port.Capacity.Multi;
        }

        public static Link.Capacity Get(this gv.Port.Capacity Capacity)
        {
            return Capacity == gv.Port.Capacity.Single ? Link.Capacity.Single : Link.Capacity.Multi;
        }
    }
}