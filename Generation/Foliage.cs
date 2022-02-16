using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.Generation
{
    [System.Serializable]
    public class Foliage : IComparable<Foliage>
    {
        public GameObject[] foliagePrefabs;
        public int renderFromDistance;
        public int amount;
        public float radius;
        public float minGreenHeight;
        public float maxGreenHeight;
        public float optimalGreenHeight;

        public float minHeight;
        public float maxHeight;
        public float maxSlope;

        public float defaultScale = 1;
        public float scalevariance = 0;

        public float seed;

        public int CompareTo(Foliage other)
        {
            if (other.minGreenHeight > this.minGreenHeight)
            {
                return 1;
            }
            else if (other.minGreenHeight < this.minGreenHeight)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }
    }
}