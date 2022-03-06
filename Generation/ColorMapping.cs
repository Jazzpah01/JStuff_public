using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.Generation
{
    public class ColorMapping : ScriptableObject
    {
        [System.Serializable]
        public class HeightToColor
        {
            public float h;
            public Color c;
        }

        public float Length
        {
            get => mapping.Length;
        }

        public HeightToColor[] mapping;

        public HeightToColor this[int i]
        {
            get => mapping[i];
        }
    }
}