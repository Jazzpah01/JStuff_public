using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace JStuff.Generation.Terrain
{
    public class TerrainObjectCollection
    {
        public List<TerrainObject> terrainObjects;
        public List<(FoliageObject, List<List<Matrix4x4>>)> foliage;

        public TerrainObjectCollection(List<TerrainObject> terrainObjects, List<(FoliageObject, List<List<Matrix4x4>>)> foliage)
        {
            this.terrainObjects = terrainObjects;
            this.foliage = foliage;
        }
    }
}
