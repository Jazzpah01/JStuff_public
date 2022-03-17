using JStuff.GraphCreator;
using JStuff.Random;
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

        PropertyPort<Vector2> chunkPosition;

        public override bool CacheOutput => true;

        protected override void SetupPorts()
        {
            seedInput = AddInputLink<int>();
            output = AddOutputLink(Evaluate);

            chunkPosition = AddPropertyLink("chunkPosition", false) as PropertyPort<Vector2>;
        }

        public int Evaluate()
        {
            return seedInput.Evaluate() * (int)chunkPosition.Evaluate().y + (int)chunkPosition.Evaluate().x;
        }
    }
}