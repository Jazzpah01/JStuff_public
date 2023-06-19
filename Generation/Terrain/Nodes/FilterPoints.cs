using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.GraphCreator;
using JStuff.Generation;
using JStuff.Utilities;

namespace JStuff.Generation.Terrain
{
    [CreateNodePath("Terrain/Points/Filter")]
    public class FilterPoints : TerrainNode
    {
        // Input
        InputLink<int> seedInput;
        InputLink<HeightMap> greenmapInput;
        InputLink<List<Vector2>> inputPoints;

        // Properties
        InputLink<float> sizeInput;
        InputLink<Vector2> positionInput;

        // Output
        OutputLink<List<Vector2>> pointsOutput;

        public override bool CacheOutput => true;

        protected override void SetupPorts()
        {
            seedInput = AddInputLink<int>();
            greenmapInput = AddInputLink<HeightMap>(portName: "HeightMap");
            inputPoints = AddInputLink<List<Vector2>>();

            sizeInput = AddPropertyInputLink<float>("chunkSize");
            positionInput = AddPropertyInputLink<Vector2>("chunkPosition");

            pointsOutput = AddOutputLink(Evaluate, portName: "List<Vector2>");
        }

        public List<Vector2> Evaluate()
        {
            System.Random rng = new System.Random(seedInput.Evaluate());
            float size = sizeInput.Evaluate();
            HeightMap gm = (greenmapInput == null) ? new HeightMap(2, 1f) : greenmapInput.Evaluate();

            List<Vector2> points = inputPoints.Evaluate();

            List<Vector2> retval = new List<Vector2>();

            foreach (Vector2 point in points)
            {
                float weight = gm.GetContinousHeight(point.x / (float)size * (gm.Width - 1), point.y / (float)size * (gm.Width - 1));

                float r = (float)rng.NextDouble();

                if (Mathf.Abs(r) % 1f < weight)
                {
                    retval.Add(point);
                }
            }

            return retval;
        }

        public override Node Clone()
        {
            FilterPoints retval = base.Clone() as FilterPoints;
            return retval;
        }
    }
}