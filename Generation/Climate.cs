using JStuff.Generation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.Generation
{
    public class Climate : ScriptableObject, IClimate
    {
        public readonly Material terrainMaterial;

        public readonly Gradient heightFoliageGradient;
        public readonly Gradient heightBarrenGradient;
        public readonly Gradient slopeGradient;

        public Subclimate subclimate;

        public Color[] GetColormap(HeightMap heightMap, HeightMap greenMap, float stretch)
        {
            throw new System.Exception();
        }
    }
}