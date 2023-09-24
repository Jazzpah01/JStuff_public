using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace JStuff.Generation.Terrain
{
    [System.Serializable]
    public struct FoliageObjectType : IObjectType
    {
        public FoliageObject foliageObject;

        [Range(0.001f, 1f)]
        public float weight;
        [Min(0)]
        public float scaleVar;
        [Min(0)]
        public float scaleChange;

        public bool rotateToTerrainNormal;

        public float Weight => weight;
        public float ScaleVar => scaleVar;
        public float ScaleChange => scaleChange;
        public bool RotateToTerrainNormal => rotateToTerrainNormal;
    }
}
