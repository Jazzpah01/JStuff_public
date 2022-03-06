using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.GraphCreator;
using JStuff.Generation;

namespace JStuff.Generation.Terrain
{
    public class EquilateralTriangleNode : Node
    {
        public int size = 65;
        public int depth = 9;
        public float zoom = 100;

        public float h = 0.35f;
        public float d = 0.35f;

        HeightMap currentHeightMap;

        InputLink<int> seedInput;
        InputLink<Vector2> offset;
        OutputLink<HeightMap> output;

        public override bool CacheOutput => true;

        protected override void SetupPorts()
        {
            seedInput = AddInputLink<int>();
            offset = AddInputLink<Vector2>();
            output = AddOutputLink(GenerateHeightMap, UnityEditor.Experimental.GraphView.Port.Capacity.Multi);
        }

        public HeightMap GenerateHeightMap()
        {
            Vector2 realOffset = offset.Evaluate() / zoom;

            EquilateralTriangle equilateralTriangle = new EquilateralTriangle();
            currentHeightMap = HeightMapGeneration.GenerateHeightmap(65, d, h, depth, seedInput.Evaluate(), zoom: zoom, offsetX: realOffset.x, offsetZ: realOffset.y, autoCache: true);
            return currentHeightMap;
        }

        public override Node Clone()
        {
            EquilateralTriangleNode retval = base.Clone() as EquilateralTriangleNode;
            retval.size = size;
            retval.depth = depth;
            retval.zoom = zoom;
            retval.h = h;
            retval.d = d;
            return retval;
        }
    }
}