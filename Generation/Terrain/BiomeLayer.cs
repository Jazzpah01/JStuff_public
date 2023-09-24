using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace JStuff.Generation.Terrain
{
    [System.Serializable]
    public class BiomeLayer
    {
        [Min(1)]
        public int terrainObjectLOD;

        [Min(0)]
        public float objectRadius;
        [Min(0)]
        public float maxSlope;

        public float minHeight;
        public float maxHeight;

        [Min(0)]
        public float scaleVar;
        [Min(0)]
        public float scaleChange;

        public List<TerrainObjectType> terrainObjectTypes = new List<TerrainObjectType>();
        public List<FoliageObjectType> foliageObjectTypes = new List<FoliageObjectType>();

        public BiomeLayer Clone()
        {
            BiomeLayer retval = new BiomeLayer();

            retval.terrainObjectLOD = terrainObjectLOD;
            retval.objectRadius = objectRadius;
            retval.maxSlope = maxSlope;
            retval.minHeight = minHeight;
            retval.maxHeight = maxHeight;
            retval.scaleVar = scaleVar;
            retval.scaleChange = scaleChange;

            retval.terrainObjectTypes = new List<TerrainObjectType>();
            for (int i = 0; i < terrainObjectTypes.Count; i++)
            {
                retval.terrainObjectTypes.Add(terrainObjectTypes[i]);
            }

            retval.foliageObjectTypes = new List<FoliageObjectType>();
            for (int i = 0; i < foliageObjectTypes.Count; i++)
            {
                retval.foliageObjectTypes.Add(foliageObjectTypes[i]);
            }

            return retval;
        }
    }
}
