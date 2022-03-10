using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.GraphCreator;
using JStuff.Generation;
using JStuff.Random;

namespace JStuff.Generation.Terrain
{
    public class Uniform2DPoints : GeneratePoints
    {
        InputLink<int> seedInput;
        InputLink<float> radiusInput;
        InputLink<float> axisSizeInput;
        OutputLink<List<Vector2>> terrainObjectsOutput;

        public override bool CacheOutput => true;

        protected override void SetupPorts()
        {
            seedInput = AddInputLink<int>();
            radiusInput = AddInputLink<float>();
            axisSizeInput = AddInputLink<float>();
            terrainObjectsOutput = AddOutputLink(Evaluate);
        }

        public List<Vector2> Evaluate()
        {
            float size = axisSizeInput.Evaluate();
            float seed = 1.0f / seedInput.Evaluate();
            float seed0 = Generator.NormalValue(seed, 0.05136f);
            return PoissonDiscSampling.GeneratePoints(seed, seed0, radiusInput.Evaluate(), new Vector2(size, size));
        }
    }
}