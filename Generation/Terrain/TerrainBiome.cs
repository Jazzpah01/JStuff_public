using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace JStuff.Generation.Terrain
{
    [System.Serializable]
    public class TerrainBiome
    {
        public List<BiomeLayer> layers;

        public TerrainBiome Clone()
        {
            TerrainBiome retval = new TerrainBiome();

            retval.layers = new List<BiomeLayer>();

            foreach (var item in layers)
            {
                retval.layers.Add(item.Clone());
            }

            return retval;
        }
    }
}
