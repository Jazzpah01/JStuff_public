using JStuff.GraphCreator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.Generation.Terrain
{
    [CreateNodePath("Terrain/HeightMap/Perlin Noise")]
    public class PerlinNoiseNode : TerrainNode
    {
        public int size = 128;
        [Min(0.00001f)]
        public float scale = 0.3f;
        [Min(1)]
        public int octaves = 1;
        [Range(0, 1)]
        public float persistance = 0.5f;
        [Min(1)]
        public float lacunarity = 2;

        InputLink<int> seedInput;
        InputLink<Vector2> offsetInput;
        InputLink<Vector2> positionInput;
        InputLink<float> chunkSizeInput;

        public override bool CacheOutput => true;

        protected override void SetupPorts()
        {
            seedInput = AddInputLink<int>();
            offsetInput = AddInputLink<Vector2>("Vector2 (optional)", inputPortSettings: InputPortSettings.Optional);
            AddOutputLink<HeightMap>(Evaluate);

            positionInput = AddPropertyInputLink<Vector2>("chunkPosition");
            chunkSizeInput = AddPropertyInputLink<float>("chunkSize");
        }

        HeightMap Evaluate()
        {
            Vector2 offset = new Vector2(0, 0);
            float chunkSize = chunkSizeInput.Evaluate();
            Vector2 position = positionInput.Evaluate();
            int seed = seedInput.Evaluate();

            if (offsetInput.LinkedPort != null)
                offset = offsetInput.Evaluate();

            //HeightMap retval = new HeightMap(PerlinNoise.GenerateNoiseMap(size, size, seedInput.Evaluate(), scale, octaves, persistance, lacunarity, offset / chunkSize + position / chunkSize));
            HeightMap retval = PerlinNoise.GenerateNoiseMap(size, size, seedInput.Evaluate(), scale, offset / chunkSize + position / chunkSize);
            return retval;
        }

        public override Node Clone()
        {
            PerlinNoiseNode retval = base.Clone() as PerlinNoiseNode;

            retval.size = size;
            retval.scale = scale;
            retval.persistance = persistance;
            retval.lacunarity = lacunarity;
            retval.octaves = octaves;

            return retval;
        }
    }
}