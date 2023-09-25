using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.GraphCreator;
using JStuff.Utilities;


namespace JStuff.Generation.Terrain
{
    [CreateNodePath("Terrain/HeightMap/Fill")]
    public class FillHeightMap : TerrainNode
    {
        // Member fields
        public int size = 65;

        public float h = 0.1f;
        public float d = 0.55f;

        public float outOfBoundsConstant = -1;

        // Input links
        InputLink<int> seedInput;
        InputLink<HeightMap> heightmapInput;

        // Property link
        InputLink<float> zoomProperty;
        InputLink<Vector2> chunkPositionProperty;
        InputLink<float> chunkSizeProperty;

        public override bool CacheOutput => true;

        protected override void SetupPorts()
        {
            seedInput = AddInputLink<int>();
            heightmapInput = AddInputLink<HeightMap>();

            zoomProperty = AddPropertyInputLink<float>("zoom");
            chunkPositionProperty = AddPropertyInputLink<Vector2>("chunkPosition");
            chunkSizeProperty = AddPropertyInputLink<float>("chunkSize");

            AddOutputLink(Evaluate);
        }

        HeightMap Evaluate()
        {
            

            int seed = seedInput.Evaluate();
            HeightMap hm = heightmapInput.Evaluate();
            float zoom = zoomProperty.Evaluate();
            float chunkSize = chunkSizeProperty.Evaluate();
            Vector2 chunkPosition = chunkPositionProperty.Evaluate();

            float inputSize = zoom / (hm.Width - 1);
            float outputSize = zoom / (size - 1);

            float distance = zoom / (hm.Width - 1);

            int inputGlobalSize = hm.Width - 1;



            float x0 = chunkPosition.x / chunkSize / zoom * (inputGlobalSize);
            float y0 = chunkPosition.y / chunkSize / zoom * (inputGlobalSize);
            float x1 = (chunkPosition.x + chunkSize) / chunkSize / zoom * (inputGlobalSize);
            float y1 = (chunkPosition.y + chunkSize) / chunkSize / zoom * (inputGlobalSize);

            if (x0 < 0 || x0 >= hm.Width-1 || y0 < 0 || y0 >= hm.Width-1)
            {
                float[,] retval = new float[size, size];

                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < size; j++)
                    {
                        retval[j, i] = outOfBoundsConstant;
                    }
                }

                return new HeightMap(retval);
            }

            Vertex va = new Vertex(x0 / (inputGlobalSize), y1 / (inputGlobalSize), hm.GetContinousHeight(x0, y1), Noise.NormalValue((float)(x0), (float)(y1)));
            Vertex vb = new Vertex(x0 / (inputGlobalSize), y0 / (inputGlobalSize), hm.GetContinousHeight(x0, y0), Noise.NormalValue((float)(x0), (float)(y0)));
            Vertex vc = new Vertex(x1 / (inputGlobalSize), y0 / (inputGlobalSize), hm.GetContinousHeight(x1, y0), Noise.NormalValue((float)(x1), (float)(y0)));
            Vertex vd = new Vertex(x1 / (inputGlobalSize), y1 / (inputGlobalSize), hm.GetContinousHeight(x1, y1), Noise.NormalValue((float)(x1), (float)(y1)));

            //Debug.Log($"{x0},{y0}: {Noise.NormalValue((float)(x0), (float)(y0))}");
            //Debug.Log($"{x1},{y0}: {Noise.NormalValue((float)(x1), (float)(y0))}");
            //Debug.Log($"{x0},{y1}: {Noise.NormalValue((float)(x0), (float)(y1))}");
            //Debug.Log($"{x1},{y1}: {Noise.NormalValue((float)(x1), (float)(y1))}");

            //RightTriangle rightTriangle = new RightTriangle(seed, size, h / (inputGlobalSize - 1), d / (inputGlobalSize - 1),
            RightTriangle rightTriangle = new RightTriangle(seed, size, h, d,
                zoom, va, vb, vc, vd);
            HeightMap currentHeightMap = rightTriangle.GetHeightMap(0, false);

            return currentHeightMap;
        }

        public override Node Clone()
        {
            FillHeightMap retval = base.Clone() as FillHeightMap;

            retval.size = size;
            retval.h = h;
            retval.d = d;

            return retval;
        }
    }
}