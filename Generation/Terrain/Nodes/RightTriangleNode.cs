using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.GraphCreator;
using JStuff.Generation;

namespace JStuff.Generation.Terrain
{
    [CreateNodePath("Terrain/HeightMap/New/Right Triangle")]
    public class RightTriangleNode : TerrainNode
    {
        public int size = 65;
        public int depth = 1;

        public float h = 0.1f;
        public float d = 0.55f;

        InputLink<int> seedInput;
        InputLink<float> zoomInput;
        InputLink<Vector2> offset;

        InputLink<Vector2> positionInput;
        InputLink<float> chunkSizeInput;

        OutputLink<HeightMap> output;

        public override bool CacheOutput => true;

        protected override void SetupPorts()
        {
            seedInput = AddInputLink<int>();
            zoomInput = AddInputLink<float>();
            offset = AddInputLink<Vector2>("Vector2 (Optional)");
            output = AddOutputLink(GenerateHeightMap);

            positionInput = AddPropertyInputLink<Vector2>("chunkPosition");
            chunkSizeInput = AddPropertyInputLink<float>("chunkSize");
        }

        static int cnt;
        public HeightMap GenerateHeightMap()
        {
            Vector2 realOffset = Vector2.zero;
            if (offset.LinkedPort != null)
            {
                realOffset += offset.Evaluate();
            }

            float chunkSize = chunkSizeInput.Evaluate();
            float zoom = zoomInput.Evaluate();
            Vector2 position = positionInput.Evaluate() / zoom / chunkSize;

            RightTriangle rightTriangle = new RightTriangle(size, h, d, zoom, realOffset + position);
            HeightMap currentHeightMap = rightTriangle.GetHeightMap(depth);
            return currentHeightMap;
        }

        public override Node Clone()
        {
            RightTriangleNode retval = base.Clone() as RightTriangleNode;
            retval.size = size;
            retval.depth = depth;
            retval.zoomInput = zoomInput;
            retval.h = h;
            retval.d = d;
            return retval;
        }
    }
}