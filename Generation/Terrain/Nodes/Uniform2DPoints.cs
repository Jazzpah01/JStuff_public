using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.GraphCreator;
using JStuff.Generation;

namespace JStuff.Generation.Terrain
{
    [CreateNodePath("Terrain/Points/New/Poisson Disc Sampling")]
    public class Uniform2DPoints : TerrainNode
    {
        public float pointDistance = 10;

        InputLink<int> seedInput;

        InputLink<float> sizeInput;

        OutputLink<List<Vector2>> points;

        public override bool CacheOutput => true;
        //public override bool IsConstant() => false;

        protected override void SetupPorts()
        {
            seedInput = AddInputLink<int>();
            sizeInput = AddPropertyInputLink<float>("chunkSize");
            points = AddOutputLink(Evaluate, portName: "List<Vector2>");
        }

        public List<Vector2> Evaluate()
        {
            float size = sizeInput.Evaluate();
            int seed = seedInput.Evaluate();
            return PoissonDiscSampling.GeneratePoints(pointDistance, new Vector2(size - 1, size - 1), seed);
        }

        public override Node Clone()
        {
            Uniform2DPoints retval = base.Clone() as Uniform2DPoints;
            retval.pointDistance = pointDistance;
            return retval;
        }
    }
}