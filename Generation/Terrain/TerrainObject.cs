using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace JStuff.Generation.Terrain
{
    public struct TerrainObject
    {
        public GameObject prefab;
        public float radius;
        public Vector3 position;
        public Quaternion rotation;
        public float scale;

        public TerrainObject(GameObject prefab, Vector3 position, float radius, float scale, Quaternion rotation)
        {
            this.prefab = prefab;
            this.position = position;
            this.rotation = rotation;
            this.radius = radius;
            this.scale = scale;
        }
        public TerrainObject(GameObject prefab, Vector3 position, float radius, float scale = 1)
        {
            this.prefab = prefab;
            this.position = position;
            this.rotation = Quaternion.identity;
            this.radius = radius;
            this.scale = scale;
        }

        //public TerrainObject(TerrainObjectType type, Vector3 pos, System.Random rng)
        //{
        //    float rot = (float)rng.NextDouble() * 360f;

        //    this.prefab = type.prefab,
        //    this.position = pos
        //    this. typeToSpawn.objectRadius,
        //            1 + typeToSpawn.scaleChange,
        //            Quaternion.Euler(0, rot, 0))
        //}
    }
}