using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.GraphCreator;

namespace JStuff.Generation.Terrain
{
    [CreateNodePath("Terrain/HeightMap/Fall Off")]
    public class FallOff : TerrainNode
    {
        // Fields
        public AnimationCurve fallOffCurve;

        // Input links
        InputLink<HeightMap> heightMapInput;
        InputLink<HeightMap> falloffMapInput;


        // Property links
        InputLink<float> zoomProperty;
        InputLink<float> chunkSizeProperty;

        InputLink<Vector2> chunkPositionProperty;

        public override bool CacheOutput => true;

        public override bool IsConstant()
        {
            return heightMapInput.IsConstant() && falloffMapInput.IsConstant();
        }



        protected override void SetupPorts()
        {
            heightMapInput = AddInputLink<HeightMap>();
            falloffMapInput = AddInputLink<HeightMap>();

            zoomProperty = AddPropertyInputLink<float>("zoom");
            chunkSizeProperty = AddPropertyInputLink<float>("chunkSize");

            chunkPositionProperty = AddPropertyInputLink<Vector2>("chunkPosition");

            AddOutputLink<HeightMap>(Evaluate);
        }

        HeightMap Evaluate()
        {
            HeightMap heightmap = heightMapInput.Evaluate();
            HeightMap falloffmap = falloffMapInput.Evaluate();

            float zoom = zoomProperty.Evaluate();
            float chunkSize = chunkSizeProperty.Evaluate();
            Vector2 chunkPosition = chunkPositionProperty.Evaluate();

            Vector2 midpoint = Vector2.one;
            float maxDistance = 1;

            if (heightmap.Global)
            {
                midpoint = Vector2.one * chunkSize / 2;
                maxDistance = chunkSize / 2;
                chunkPosition = Vector2.zero;
                zoom = 1;
            } else
            {
                midpoint = Vector2.one * chunkSize * zoom / 2;
                maxDistance = chunkSize * zoom / 2;
            }

            float[,] retval = new float[heightmap.Width, heightmap.Length];

            for (int i = 0; i < heightmap.Width; i++)
            {
                for (int j = 0; j < heightmap.Width; j++)
                {
                    float normX = (float)i / (heightmap.Width-1);
                    float normY = (float)j / (heightmap.Length-1);
                    float v0 = heightmap[i, j];
                    float v1 = falloffmap.GetContinousHeight(normX * (falloffmap.Width-1),
                                                            normY * (falloffmap.Length-1));

                    float t = Vector2.Distance(new Vector2(normX, normY) * chunkSize + chunkPosition, midpoint);
                    t /= maxDistance;
                    t = Mathf.Clamp01(t);
                    t = fallOffCurve.Evaluate(1 - t);
                    t = Mathf.Clamp01(t);

                    retval[i, j] = Mathf.Lerp(v1, v0, t);
                }
            }

            return new HeightMap(retval);
        }

        public override Node Clone()
        {
            FallOff retval = base.Clone() as FallOff;
            retval.fallOffCurve = new AnimationCurve(fallOffCurve.keys);
            return retval;
        }
    }
}