using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.Generation.Terrain
{
    public struct TerrainObject
    {
        public GameObject prefab;
        public Vector3 position;
        public Quaternion rotation;

        public TerrainObject(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            this.prefab = prefab;
            this.position = position;
            this.rotation = rotation;
        }
        public TerrainObject(GameObject prefab, Vector3 position)
        {
            this.prefab = prefab;
            this.position = position;
            rotation = Quaternion.identity;
        }
    }
}