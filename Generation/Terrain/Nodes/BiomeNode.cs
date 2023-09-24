using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.GraphCreator;
using JStuff.Utilities;
using System.Runtime.InteropServices.WindowsRuntime;

namespace JStuff.Generation.Terrain
{
    [CreateNodePath("Terrain/Terrain Objects/New Biome")]
    public class BiomeNode : TerrainNode
    {
        public TerrainBiome biome;

        protected override void SetupPorts()
        {
            AddOutputLink<TerrainBiome>(() => biome, portName: "Biome");
        }

        public override Node Clone()
        {
            var retval = base.Clone() as BiomeNode;

            retval.biome = biome.Clone();

            return retval;
        }
    }
}