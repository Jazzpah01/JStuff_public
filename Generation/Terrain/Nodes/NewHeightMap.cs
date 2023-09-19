using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.Generation.Terrain
{
    public abstract class NewHeightMap : TerrainNode
    {
        [Header("Visualization")]
        public int visualizationSeed = -1;
        public int targetVisualizationSeed = -1;
        public Texture2D heightmapTexture;

        public abstract Texture2D GetTexture();
    }
}