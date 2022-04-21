using JStuff.Dialogue;
using JStuff.GraphCreator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.Generation.Terrain
{
    public class LayeredPerlinNoiseNode : Node
    {
        public int size = 128;
        [Min(1)]
        public int octaves = 1;
        [Range(0, 1)]
        public float persistance = 0.5f;
        [Min(1)]
        public float lacunarity = 2;
        [Min(0.00001f)]
        public float scale = 0.3f;

        InputLink<int> seedInput;
        InputLink<float> scaleInput;
        InputLink<Vector2> offsetInput;
        InputLink<Vector2> positionInput;
        InputLink<float> chunkSizeInput;

        public override bool CacheOutput => true;

        protected override void SetupPorts()
        {
            seedInput = AddInputLink<int>();
            scaleInput = AddInputLink<float>();
            offsetInput = AddInputLink<Vector2>("Vector2 (optional)");
            positionInput = AddPropertyInputLink<Vector2>("chunkPosition");
            chunkSizeInput = AddPropertyInputLink<float>("chunkSize");
            AddOutputLink<HeightMap>(Evaluate);
        }

        HeightMap Evaluate()
        {
            float scale = scaleInput.Evaluate();
            float chunkSize = chunkSizeInput.Evaluate();
            Vector2 position = positionInput.Evaluate();
            Vector2 offset = offsetInput.Evaluate();

            HeightMap retval = new HeightMap(PerlinNoise.GenerateNoiseMap(size, size, seedInput.Evaluate(), scale, octaves, persistance, lacunarity, offset / chunkSize + position / chunkSize));
            return retval;
        }

        public override Node Clone()
        {
            LayeredPerlinNoiseNode retval = base.Clone() as LayeredPerlinNoiseNode;

            retval.size = size;
            retval.octaves = octaves;
            retval.persistance = persistance;
            retval.lacunarity = lacunarity;

            return retval;
        }
    }
}