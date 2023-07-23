using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.Generation
{
    public static class TerrainMeshGeneration
    {
        public static MeshData GenerateMeshData(HeightMap map, float chunksize, float multiplier = 1)
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

            return new MeshData(vertices, uv, triangles, multiplier, map.Length, map.Width);
        }

        public static MeshData GenerateLODMeshData(MeshData meshData, int LOD)
        {
            int inputWidth = meshData.Width();

            if (((inputWidth - 1) / LOD - (int)(inputWidth - 1) / LOD) != 0)
                throw new System.Exception("Error: LOD doesn't fit.");

            int newWidth = (inputWidth - 1) / LOD + 1;

            Vector3[] vertices = new Vector3[newWidth * newWidth];
            Vector2[] uv = new Vector2[vertices.Length];
            int[] triangles = new int[(newWidth - 1) * (newWidth - 1) * 6];

            int i = 0;

            for (int x = 0; x < newWidth; x++)
            {
                for (int z = 0; z < newWidth; z++)
                {
                    vertices[x + z * newWidth] = meshData.vertices[(x * LOD) + (z * LOD) * inputWidth];
                    uv[x + z * newWidth] = new Vector2((float)x / newWidth, (float)z / newWidth);

                    if (x != newWidth - 1 && z != newWidth - 1)
                    {
                        triangles[i] = x + newWidth * z;
                        triangles[i + 1] = x + newWidth * (z + 1);
                        triangles[i + 2] = x + 1 + newWidth * (z + 1);

                        triangles[i + 3] = x + newWidth * z;
                        triangles[i + 4] = x + 1 + newWidth * (z + 1);
                        triangles[i + 5] = x + 1 + newWidth * z;

                        i += 6;
                    }
                }
            }

            return new MeshData(vertices, uv, triangles, meshData.heightFactor, newWidth, newWidth);
        }

        public static Mesh GenerateMesh(MeshData inputMeshData, Color[] colormap, int LOD = 1)
        {
            MeshData meshData = inputMeshData;

            if (LOD > 1)
            {
                meshData = GenerateLODMeshData(inputMeshData, LOD);
            }

            if (colormap != null)
            {
                Mesh mesh = new Mesh();
                mesh.vertices = meshData.vertices;
                mesh.uv = meshData.uv;
                mesh.triangles = meshData.triangles;
                mesh.colors = colormap;
                mesh.RecalculateNormals();
                mesh.RecalculateTangents();

                return mesh;
            } else
            {
                Mesh mesh = new Mesh();
                mesh.vertices = meshData.vertices;
                mesh.uv = meshData.uv;
                mesh.triangles = meshData.triangles;

                return mesh;
            }
        }

        public static Color[] GenerateLODColormap(Color[] colormap, int LOD = 1)
        {
            if (LOD == 1)
                return colormap;

            int inputWidth = (int)Mathf.Sqrt(colormap.Length);

            if (((inputWidth - 1) / LOD - (int)(inputWidth - 1) / LOD) != 0)
                throw new System.Exception("Error: LOD doesn't fit.");

            int newWidth = (inputWidth - 1) / LOD + 1;

            Color[] retval = new Color[newWidth * newWidth];

            for (int x = 0; x < newWidth; x++)
            {
                for (int z = 0; z < newWidth; z++)
                {
                    retval[x + z * newWidth] = colormap[(x * LOD) + (z * LOD) * inputWidth];
                }
            }

            return retval;
        }
    }

    

    public class MeshData
    {
        public MeshData(Vector3[] vertices, Vector2[] uv, int[] triangles, float heightFactor, int sizeX, int sizeZ)
        {
            this.vertices = vertices;
            this.uv = uv;
            this.triangles = triangles;
            this.heightFactor = heightFactor;
            this.sizeX = sizeX;
            this.sizeZ = sizeZ;
        }
        public Vector3[] vertices;
        public Vector2[] uv;
        public int[] triangles;
        public float heightFactor;
        public int sizeX;
        public int sizeZ;

        public void CalculateUnormalizedNormals(ref Vector3[] normals)
        {
            if (normals.Length != vertices.Length)
                throw new System.Exception($"Length of normals ({normals.Length}) must be equal to Length of vertices ({vertices.Length})");

            for (int i = 0; i < normals.Length; i++)
            {
                normals[i] = Vector3.zero;
            }

            int triangleCount = triangles.Length / 3;
            for (int i = 0; i < triangleCount; i++)
            {
                int normalTriangleIndex = i * 3;
                int vertexIndexA = triangles[normalTriangleIndex];
                int vertexIndexB = triangles[normalTriangleIndex + 1];
                int vertexIndexC = triangles[normalTriangleIndex + 2];

                Vector3 surfaceNormal = SurfaceNormal(vertexIndexA, vertexIndexB, vertexIndexC);
                normals[vertexIndexA] += surfaceNormal;
                normals[vertexIndexB] += surfaceNormal;
                normals[vertexIndexC] += surfaceNormal;
            }
        }

        public Vector3 SurfaceNormal(int indexA, int indexB, int indexC)
        {
            Vector3 a = vertices[indexA];
            Vector3 b = vertices[indexB];
            Vector3 c = vertices[indexC];

            return Vector3.Cross(b - a, c - a).normalized;
        }

        public int Width()
        {
            return (int)Mathf.Sqrt(vertices.Length);
        }

        public MeshData Clone()
        {
            Vector3[] vertices = new Vector3[this.vertices.Length];
            Vector2[] uv = new Vector2[this.uv.Length];
            int[] triangles = new int[this.triangles.Length];

            int size = Mathf.RoundToInt(Mathf.Sqrt((float)this.vertices.Length));

            int i = 0;

            for (int z = 0; z < size; z++)
            {
                for (int x = 0; x < size; x++)
                {
                    vertices[x + z * size] = this.vertices[x + z * size];
                    uv[x + z * size] = this.uv[x + z * size];

                    if (x != size - 1 && z != size - 1)
                    {
                        triangles[i] = this.triangles[i];
                        triangles[i + 1] = this.triangles[i + 1];
                        triangles[i + 2] = this.triangles[i + 2];

                        triangles[i + 3] = this.triangles[i + 3];
                        triangles[i + 4] = this.triangles[i + 4];
                        triangles[i + 5] = this.triangles[i + 5];

                        i += 6;
                    }
                }
            }

            return new MeshData(vertices, uv, triangles, this.heightFactor, this.sizeX, this.sizeZ);
        }

        public (int, int) GetXZ(int vertexIndex)
        {
            return (vertexIndex % sizeX, vertexIndex / sizeZ);
        }
    }
}