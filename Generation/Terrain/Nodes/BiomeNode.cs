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
        public List<TerrainObjectType> biome;
        private List<IWeightedPrefab> prefabs;

        protected override void SetupPorts()
        {
            AddOutputLink<List<IWeightedPrefab>>(() => prefabs, portName: "Biome");
        }

        public override void Initialize()
        {
            prefabs = new List<IWeightedPrefab>();

            foreach (TerrainObjectType item in biome)
            {
                prefabs.Add(item);
            }
        }

        public override Node Clone()
        {
            var retval = base.Clone() as BiomeNode;

            List<TerrainObjectType> biomeClone = new List<TerrainObjectType>();

            foreach (var item in biome)
            {
                biomeClone.Add(item);
            }

            retval.biome = biomeClone;
            return retval;
        }
    }
}