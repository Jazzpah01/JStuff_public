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
        public float radius;

        public TerrainObject(GameObject prefab, Vector3 position, float radius, Quaternion rotation)
        {
            this.prefab = prefab;
            this.position = position;
            this.rotation = rotation;
            this.radius = radius;
        }
        public TerrainObject(GameObject prefab, Vector3 position, float radius)
        {
            this.prefab = prefab;
            this.position = position;
            this.rotation = Quaternion.identity;
            this.radius = radius;
        }
    }
}