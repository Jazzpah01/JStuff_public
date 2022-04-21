using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.GraphCreator;
using JStuff.Generation;
using JStuff.Random;
using JStuff.Dialogue;

namespace JStuff.Generation.Terrain
{
    [CreateNodePath("Terrain/Points/Uniform")]
    public class Uniform2DPoints : TerrainNode
    {
        public float pointDistance = 10;

        InputLink<int> seedInput;

        InputLink<float> sizeInput;

        OutputLink<List<Vector2>> terrainObjectsOutput;

        public override bool CacheOutput => true;

        protected override void SetupPorts()
        {
            seedInput = AddInputLink<int>();
            sizeInput = AddPropertyInputLink<float>("chunkSize");
            terrainObjectsOutput = AddOutputLink(Evaluate, portName: "List<Vector2>");
        }

        public List<Vector2> Evaluate()
        {
            float size = sizeInput.Evaluate();
            float seed = 1.0f / seedInput.Evaluate();
            float seed0 = Generator.NormalValue(seed, 0.05136f);
            return PoissonDiscSampling.GeneratePoints(seed, seed0, pointDistance, new Vector2(size, size));
        }

        public override Node Clone()
        {
            Uniform2DPoints retval = base.Clone() as Uniform2DPoints;
            retval.pointDistance = pointDistance;
            return retval;
        }
    }
}