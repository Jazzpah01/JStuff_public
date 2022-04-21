using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.GraphCreator;
using JStuff.Generation;
using JStuff.Random;
using JStuff.Dialogue;

namespace JStuff.Generation.Terrain
{
    [CreateNodePath("Terrain/Points/Filter")]
    public class FilterPoints : TerrainNode
    {
        public float maxHeight;
        public float minHeight;

        public float factor;

        public float maxSlope;

        public AnimationCurve curve;

        InputLink<int> seedInput;
        InputLink<HeightMap> greenmapInput;
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
            greenmapInput = AddInputLink<HeightMap>(inputPortSettings: InputPortSettings.Optional);
            inputPoints = AddInputLink<List<Vector2>>();
            scaleInput = AddInputLink<float>();

            sizeInput = AddPropertyInputLink<float>("chunkSize");
            positionInput = AddPropertyInputLink<Vector2>("chunkPosition");

            pointsOutput = AddOutputLink(Evaluate, portName: "List<Vector2>");
        }

        public List<Vector2> Evaluate()
        {
            if (greenmapInput.linkedPort == null)
            {
                return Evaluate_WithoutGreenmap();
            }

            System.Random rng = new System.Random(seedInput.Evaluate());
            float size = sizeInput.Evaluate();
            HeightMap gm = greenmapInput.Evaluate();
            HeightMap hm = heightmapInput.Evaluate();
            List<Vector2> points = inputPoints.Evaluate();
            float scale = scaleInput.Evaluate();

            List<Vector2> retval = new List<Vector2>();

            foreach (Vector2 point in points)
            {
                float weight = gm.GetContinousHeight(point.x / (float)size, point.y / (float)size);
                float height = hm.GetContinousHeight(point.x / (float)size, point.y / (float)size);

                float slope = hm.GetSlope(point.x / (float)size, point.y / (float)size) * scale;
                slope = 0;
                if (height < minHeight || height > maxHeight || slope > maxSlope)
                    continue;

                weight = weight.Remap(-1, 1, 0, 1);
                height = height.Remap(minHeight, maxHeight, 0, 1);
                height = curve.Evaluate(height);

                float r = (float)rng.NextDouble();

                if (Mathf.Abs(r) % 1f < weight * height * factor)
                {
                    retval.Add(point);
                }
            }

            return retval;
        }

        public List<Vector2> Evaluate_WithoutGreenmap()
        {
            System.Random rng = new System.Random(seedInput.Evaluate());
            float size = sizeInput.Evaluate();
            HeightMap hm2 = heightmapInput.Evaluate();
            List<Vector2> points = inputPoints.Evaluate();
            float scale = scaleInput.Evaluate();

            List<Vector2> retval = new List<Vector2>();

            foreach (Vector2 point in points)
            {
                float height = hm2.GetContinousHeight(point.x / (float)size, point.y / (float)size);

                float slope = hm2.GetSlope(point.x / (float)size, point.y / (float)size) * scale;
                if (height < minHeight || height > maxHeight || slope > maxSlope)
                    continue;

                height = height.Remap(minHeight, maxHeight, 0, 1);
                height = curve.Evaluate(height);

                float r = (float)rng.NextDouble();

                if (Mathf.Abs(r) % 1f < height * factor)
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
            retval.factor = factor;
            retval.maxHeight = maxHeight;

            return retval;
        }
    }
}