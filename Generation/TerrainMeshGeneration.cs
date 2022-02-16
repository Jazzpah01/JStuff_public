using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.Generation
{
    public static class TerrainMeshGeneration
    {
        public static MeshData GenerateMesh(HeightMap map, float chunksize, float multiplier = 1)
        {
            Vector3[] vertices = new Vector3[map.Width * map.Length];
            Vector2[] uv = new Vector2[vertices.Length];
            int[] triangles = new int[(map.Width - 1) * (map.Length - 1) * 6];

            int i = 0;

            for (int z = 0; z < map.Length; z++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    vertices[x + z * map.Width] = new Vector3(x * (chunksize / (map.Width - 1)), map[x, z] * multiplier, z * (chunksize / (map.Width - 1)));
                    uv[x + z * map.Width] = new Vector2((float)x / map.Width, (float)z / map.Length);

                    if (x != map.Width - 1 && z != map.Length - 1)
                    {
                        triangles[i] = x + map.Width * z;
                        triangles[i + 1] = x + map.Width * (z + 1);
                        triangles[i + 2] = x + 1 + map.Width * (z + 1);

                        triangles[i + 3] = x + map.Width * z;
                        triangles[i + 4] = x + 1 + map.Width * (z + 1);
                        triangles[i + 5] = x + 1 + map.Width * z;

                        i += 6;
                    }
                }
            }

            return new MeshData(vertices, uv, triangles);
        }
    }

    public struct MeshData
    {
        public MeshData(Vector3[] vertices, Vector2[] uv, int[] triangles)
        {
            this.vertices = vertices;
            this.uv = uv;
            this.triangles = triangles;
        }
        public Vector3[] vertices;
        public Vector2[] uv;
        public int[] triangles;
    }
}