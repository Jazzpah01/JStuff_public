using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.GraphCreator;
using JStuff.Generation;
using JStuff.Random;

namespace JStuff.Generation.Terrain
{
    [CreateNodePath("Terrain/Points/Filter")]
    public class FilterPoints : TerrainNode
    {
        public float maxHeight;
        public float minHeight;

        public AnimationCurve curve;

        InputLink<int> seedInput;
        InputLink<HeightMap> heightmapInput;
        InputLink<List<Vector2>> inputPoints;
        InputLink<float> scaleInput;

        InputLink<float> sizeInput;
        InputLink<Vector2> positionInput;

        OutputLink<List<Vector2>> pointsOutput;

        public override bool CacheOutput => true;

        protected override void SetupPorts()
        {
            seedInput = AddInputLink<int>();
            heightmapInput = AddInputLink<HeightMap>();
            inputPoints = AddInputLink<List<Vector2>>();
            scaleInput = AddInputLink<float>();

            sizeInput = AddPropertyInputLink<float>("chunkSize");
            positionInput = AddPropertyInputLink<Vector2>("chunkPosition");

            pointsOutput = AddOutputLink(Evaluate, portName: "List<Vector2>");
        }

        public List<Vector2> Evaluate()
        {
            System.Random rng = new System.Random(seedInput.Evaluate());
            float size = sizeInput.Evaluate();
            float scale = scaleInput.Evaluate();
            HeightMap hm = heightmapInput.Evaluate();
            List<Vector2> points = inputPoints.Evaluate();
            Vector2 position = positionInput.Evaluate();

            List<Vector2> retval = new List<Vector2>();

            foreach (Vector2 point in points)
            {
                float height = hm.GetContinousHeight(point.x / (float)size, point.y / (float)size);

                if (height < minHeight || height > maxHeight)
                    continue;

                float converted = curve.Evaluate(height.Remap(minHeight, maxHeight, 0, 1));
                float r = (float)rng.NextDouble();
                if (Mathf.Abs(r) % 1f < converted.Clamp(0,1))
                {
                    retval.Add(point);
                }
            }

            return retval;
        }

        public override Node Clone()
        {
            FilterPoints retval = base.Clone() as FilterPoints;

            AnimationCurve curve = new AnimationCurve(this.curve.keys);

            retval.curve = curve;
            retval.maxHeight = maxHeight;
            retval.minHeight = minHeight;

            return retval;
        }
    }
}