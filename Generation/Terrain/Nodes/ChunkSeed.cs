using JStuff.GraphCreator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.Generation.Terrain
{
    [CreateNodePath("Terrain/Utility/Chunk seed")]
    public class ChunkSeed : TerrainNode
    {
        InputLink<int> seedInput;
        OutputLink<int> output;

        InputLink<Vector2> chunkPosition;

        public override bool CacheOutput => true;

        protected override void SetupPorts()
        {
            seedInput = AddInputLink<int>();
            output = AddOutputLink(Evaluate);

            chunkPosition = AddPropertyInputLink<Vector2>("chunkPosition");
        }

        public int Evaluate()
        {
            return seedInput.Evaluate() * (int)chunkPosition.Evaluate().y + (int)chunkPosition.Evaluate().x;
        }
    }
}