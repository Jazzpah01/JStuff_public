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
        public float maxSlope = 1;
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
                float slope = hm.GetSlope(point.x / size, point.y / size);

                if (slope > maxSlope)
                    continue;

                float converted = curve.Evaluate((hm.GetContinousHeight(point.x / size, point.y / size) + 1) / 2) * 2 - 1;
                if (rng.NextDouble() % 1f < converted)
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

            retval.maxSlope = maxSlope;
            retval.curve = curve;

            return retval;
        }
    }
}