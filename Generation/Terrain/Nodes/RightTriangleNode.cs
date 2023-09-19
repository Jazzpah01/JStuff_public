using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.GraphCreator;
using JStuff.Generation;
using UnityEngine.UIElements;

namespace JStuff.Generation.Terrain
{
    [CreateNodePath("Terrain/HeightMap/New/Right Triangle")]
    public class RightTriangleNode : NewHeightMap
    {

        [Header("Node Attributes")]
        public bool global = false;
        public bool zoomable = true;

        public int size = 65;
        public int depth = 1;

        public float h = 0.1f;
        public float d = 0.55f;

        InputLink<int> seedInput;
        InputLink<float> zoomInput;
        InputLink<Vector2> offset;

        InputLink<Vector2> positionProperty;
        InputLink<float> chunkSizeProperty;

        OutputLink<HeightMap> output;

        public override bool CacheOutput => true;

        public override bool IsConstant() => global;

        protected override void SetupPorts()
        {
            seedInput = AddInputLink<int>();
            zoomInput = AddInputLink<float>();
            offset = AddInputLink<Vector2>("Vector2 (Optional)");
            output = AddOutputLink(GenerateHeightMap);

            positionProperty = AddPropertyInputLink<Vector2>("chunkPosition");
            chunkSizeProperty = AddPropertyInputLink<float>("chunkSize");
        }

        public override void Initialize()
        {
            heightmapTexture = null;
        }

        public HeightMap GenerateHeightMap()
        {
            Vector2 realOffset = Vector2.zero;
            if (offset.LinkedPort != null)
            {
                realOffset += offset.Evaluate();
            }

            float chunkSize = chunkSizeProperty.Evaluate();
            float zoom = zoomInput.Evaluate();
            Vector2 position = positionProperty.Evaluate();

            if (global)
            {
                zoom = 1;
                position = new Vector2(0, 0);
            }
                

            RightTriangle rightTriangle = new RightTriangle(seedInput.Evaluate(), size, h, d, zoom, (realOffset + position) / zoom / chunkSize);
            HeightMap currentHeightMap = rightTriangle.GetHeightMap(depth, zoomable);

            if (this.global)
                currentHeightMap.Global = this.global;

            return currentHeightMap;
        }

        public override Node Clone()
        {
            RightTriangleNode retval = base.Clone() as RightTriangleNode;
            retval.global = global;
            retval.size = size;
            retval.depth = depth;
            retval.h = h;
            retval.d = d;
            retval.zoomable = zoomable;
            return retval;
        }

        public override Texture2D GetTexture()
        {
            TerrainGraph g = ((TerrainGraph)graph);

            if (g == null)
                return null;

            if (targetVisualizationSeed == -1)
            {
                if (visualizationSeed != g.seed || heightmapTexture == null)
                {
                    visualizationSeed = g.seed;

                    RightTriangle rightTriangle = new RightTriangle(visualizationSeed, 129, h, d, 1, Vector2.zero);
                    HeightMap hm = rightTriangle.GetHeightMap(15);

                    heightmapTexture = hm.ToTexture();
                }
            } else
            {
                if (visualizationSeed != targetVisualizationSeed || heightmapTexture == null)
                {
                    visualizationSeed = targetVisualizationSeed;

                    RightTriangle rightTriangle = new RightTriangle(visualizationSeed, 129, h, d, 1, Vector2.zero);
                    HeightMap hm = rightTriangle.GetHeightMap(15);

                    heightmapTexture = hm.ToTexture();
                }
            }

            

            return heightmapTexture;
        }
    }
}