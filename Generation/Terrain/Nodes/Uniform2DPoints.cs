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
        InputLink<float> sizeInput;
        InputLink<float> objectRadiusInput;
        OutputLink<List<Vector2>> terrainObjectsOutput;

        public override bool CacheOutput => true;

        protected override void SetupPorts()
        {
            seedInput = AddInputLink<int>();
            sizeInput = AddInputLink<float>();
            objectRadiusInput = AddInputLink<float>();
            terrainObjectsOutput = AddOutputLink(Evaluate, portName: "List<Vector2>");
        }

        public List<Vector2> Evaluate()
        {
            float size = sizeInput.Evaluate();
            float seed = 1.0f / seedInput.Evaluate();
            float seed0 = Generator.NormalValue(seed, 0.05136f);
            return PoissonDiscSampling.GeneratePoints(seed, seed0, objectRadiusInput.Evaluate(), new Vector2(size, size));
        }
    }
}