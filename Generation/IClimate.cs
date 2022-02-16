using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.Generation
{
    public interface IClimate
    {
        Color[] GetColormap(HeightMap heightMap, HeightMap greenMap, float heightmapStretch);
    }
}