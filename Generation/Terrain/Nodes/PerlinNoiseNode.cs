using JStuff.GraphCreator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.Generation.Terrain
{
    public class PerlinNoiseNode : GenerateHeightMap
    {
        public int size = 128;
        public int octaves = 0;
        [Range(0, 1)]
        public float persistance = 0.5f;
        public float lacunarity = 2;

        InputLink<int> seedInput;
        InputLink<float> chunkSizeInput;
        InputLink<Vector2> offsetInput;

        protected override void SetupPorts()
        {
            seedInput = AddInputLink<int>();
            chunkSizeInput = AddInputLink<float>();
            offsetInput = AddInputLink<Vector2>();
            AddOutputLink<HeightMap>(Evaluate);
        }

        HeightMap Evaluate()
        {
            HeightMap retval = PerlinNoise.GenerateNoiseMap(size, size, seedInput.Evaluate(), chunkSizeInput.Evaluate(), octaves, persistance, lacunarity, offsetInput.Evaluate());
            return retval;
        }

        public override Node Clone()
        {
            PerlinNoiseNode retval = base.Clone() as PerlinNoiseNode;

            retval.size = size;
            retval.octaves = octaves;
            retval.persistance = persistance;
            retval.lacunarity = lacunarity;

            return retval;
        }
    }
}