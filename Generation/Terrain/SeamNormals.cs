using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.Utilities;
using JStuff.Generation;

public class Seam
{
    public int Length;
    public Color[] seamColors;
    public Vector3[] unormalized_normals;
    public float[] positions;

    //public Seam(Vector3[] unormalizedMeshNormals, Color[] colormap, int direction)
    //{
    //    Length = (int) Mathf.Sqrt(unormalizedMeshNormals.Length);
    //    seamColors = new Color[Length];
    //    unormalized_normals = new Vector3[Length];
    //    positions = new float[Length];

    //    if (direction == 0)
    //    {
    //        // Right
    //        for (int i = 0; i < Length; i++)
    //        {
    //            int meshIndex = i * Length + Length - 1;

    //            unormalized_normals[i] = unormalizedMeshNormals[meshIndex];
    //            seamColors[i] = colormap[meshIndex];
    //        }
    //    } else if (direction == 1)
    //    {
    //        // Up
    //        for (int i = 0; i < Length; i++)
    //        {
    //            int meshIndex = i + Length * Length - Length;

    //            unormalized_normals[i] = unormalizedMeshNormals[meshIndex];
    //            seamColors[i] = colormap[meshIndex];
    //        }
    //    } else if (direction == 2)
    //    {
    //        // Left
    //        for (int i = 0; i < Length; i++)
    //        {
    //            int meshIndex = i * Length;

    //            unormalized_normals[i] = unormalizedMeshNormals[meshIndex];
    //            seamColors[i] = colormap[meshIndex];
    //        }
    //    } else if (direction == 3)
    //    {
    //        // Down
    //        for (int i = 0; i < Length; i++)
    //        {
    //            int meshIndex = i;

    //            unormalized_normals[i] = unormalizedMeshNormals[meshIndex];
    //            seamColors[i] = colormap[meshIndex];
    //        }
    //    }
    //}

    //public Seam(Color[] seamColors, Vector3[] unormalized_normals, float[] positions)
    //{
    //    if (seamColors == null || unormalized_normals == null || positions == null ||
    //        seamColors.Length != unormalized_normals.Length || seamColors.Length != positions.Length)
    //        throw new System.Exception();

    //    this.seamColors = seamColors;
    //    this.unormalized_normals = unormalized_normals;
    //    this.positions = positions;

    //    this.Length = positions.Length;
    //}

    //public bool SharedAmount(Seam other)
    //{
    //    return other.positions.Length == positions.Length;
    //}

    public static void UpdateNormals(ref Vector3[] normals, Vector3[] otherSeamNormals, int direction)
    {
        int thisNormalSeamCount = (int)Mathf.Sqrt(normals.Length);
        int otherNormalSeamCount = otherSeamNormals.Length;

        int thisScale = 1 / (thisNormalSeamCount - 1);
        int otherScale = 1 / (otherNormalSeamCount - 1);

        if (thisNormalSeamCount == otherNormalSeamCount)
        {
            for (int i = 0; i < thisNormalSeamCount; i++)
            {
                int meshIndex = SeamToArrayIndex(thisNormalSeamCount, direction, i);

                normals[meshIndex] = otherSeamNormals[i];
            }
        }
        if (thisNormalSeamCount < otherNormalSeamCount)
        {
            // No interpolation
            int j = 0;
            for (int i = 0; i < thisNormalSeamCount; i++)
            {
                while((j - 1) * otherScale < (i - 1) * thisScale && Mathf.Abs((j - 1) * otherScale - (i - 1) * thisScale) > 0.0001f)
                {
                    j++;
                }

                int meshIndex = SeamToArrayIndex(thisNormalSeamCount, direction, i);

                normals[meshIndex] += otherSeamNormals[j];
            }
        } else if (thisNormalSeamCount > otherNormalSeamCount)
        {
            // Needs interpolation
            int j = 0;
            for (int i = 0; i < thisNormalSeamCount; i++)
            {
                while ((j - 1) * otherScale < (i - 1) * thisScale && Mathf.Abs((j - 1) * otherScale - (i - 1) * thisScale) > 0.0001f)
                {
                    j++;
                }

                int meshIndex = SeamToArrayIndex(thisNormalSeamCount, direction, i);

                if (Mathf.Abs((j - 1f) * otherScale - (i - 1f) * thisScale) < 0.0001f)
                {
                    // same index
                    normals[meshIndex] += otherSeamNormals[j];
                } else
                {
                    float t = ((i - 1f) * thisScale).Remap((j - 2f) * otherScale, (j - 1f) * otherScale, 0f, 1f);
                    normals[meshIndex] += otherSeamNormals[j - 1] * (1-t) + otherSeamNormals[j] * t;
                }
            }
        }
    }

    public static void UpdateColors(ref Color[] colormap, Color[] seamColormap, int direction)
    {
        if (direction < 0 || direction >= 4)
        {
            throw new System.Exception();
        }

        int thisNormalSeamCount = (int)Mathf.Sqrt(colormap.Length);
        int otherNormalSeamCount = seamColormap.Length;

        int thisScale = 1 / (thisNormalSeamCount - 1);
        int otherScale = 1 / (otherNormalSeamCount - 1);

        if (thisNormalSeamCount == otherNormalSeamCount)
        {
            for (int i = 0; i < thisNormalSeamCount; i++)
            {
                int meshIndex = SeamToArrayIndex(thisNormalSeamCount, direction, i);

                colormap[meshIndex] = (colormap[meshIndex] + seamColormap[i]) / 2f;
            }
        }
        if (thisNormalSeamCount < otherNormalSeamCount)
        {
            // No interpolation
            int j = 0;
            for (int i = 0; i < thisNormalSeamCount; i++)
            {
                while ((j - 1) * otherScale < (i - 1) * thisScale && Mathf.Abs((j - 1) * otherScale - (i - 1) * thisScale) > 0.0001f)
                {
                    j++;
                }

                int meshIndex = SeamToArrayIndex(thisNormalSeamCount, direction, i);

                colormap[meshIndex] = (colormap[meshIndex] + seamColormap[j]) / 2f;
            }
        }
        else if (thisNormalSeamCount > otherNormalSeamCount)
        {
            // Needs interpolation
            int j = 0;
            for (int i = 0; i < thisNormalSeamCount; i++)
            {
                while ((j - 1) * otherScale < (i - 1) * thisScale && Mathf.Abs((j - 1) * otherScale - (i - 1) * thisScale) > 0.0001f)
                {
                    j++;
                }

                int meshIndex = SeamToArrayIndex(thisNormalSeamCount, direction, i);

                if (Mathf.Abs((j - 1f) * otherScale - (i - 1f) * thisScale) < 0.0001f)
                {
                    // same index
                    colormap[meshIndex] += seamColormap[j];
                }
                else
                {
                    float t = ((i - 1f) * thisScale).Remap((j - 2f) * otherScale, (j - 1f) * otherScale, 0f, 1f);
                    colormap[meshIndex] = (colormap[meshIndex] + seamColormap[j - 1] * (1 - t) + seamColormap[j] * t) / 2;
                }
            }
        }
    }

    public static void UpdateSeamNormals(MeshData thisMesh, MeshData otherMesh, ref Vector3[] normals, int direction)
    {
        if (normals.Length != thisMesh.vertices.Length)
            throw new System.Exception($"Length of normals ({normals.Length}) must be equal to Length of vertices ({thisMesh.vertices.Length})");

        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = Vector3.zero;
        }

        int otherDireciton = (direction + 2) % 4;

        int triangleCount = otherMesh.triangles.Length / 3;

        int seamSquareCount = Mathf.RoundToInt(Mathf.Sqrt(triangleCount / 2));

        // Get seam normals of other
        Vector3[] otherSeamNormals = new Vector3[otherMesh.sizeX];
        for (int i = 0; i < seamSquareCount; i++)
        {
            int squareIndex = SeamToArrayIndex(seamSquareCount, otherDireciton, i);

            int normalTriangleIndex = squareIndex * 2 * 3;

            int vertexIndexA_0 = otherMesh.triangles[normalTriangleIndex];
            int vertexIndexB_0 = otherMesh.triangles[normalTriangleIndex + 1];
            int vertexIndexC_0 = otherMesh.triangles[normalTriangleIndex + 2];

            Vector3 surfaceNormal = otherMesh.SurfaceNormal(vertexIndexA_0, vertexIndexB_0, vertexIndexC_0);

            foreach (var item in new int[] { vertexIndexA_0, vertexIndexB_0, vertexIndexC_0 })
            {
                (int x, int z) = otherMesh.GetXZ(item);

                if ((otherDireciton == 0 || otherDireciton == 2) && (x == 0 || x == otherMesh.sizeX - 1))
                {
                    // Horizontal
                    otherSeamNormals[z] += surfaceNormal;
                }
                else if ((otherDireciton == 1 || otherDireciton == 3) && (z == 0 || z == otherMesh.sizeZ - 1))
                {
                    // Vertical
                    otherSeamNormals[x] += surfaceNormal;
                }
            }

            normalTriangleIndex += 3;

            int vertexIndexA_1 = otherMesh.triangles[normalTriangleIndex];
            int vertexIndexB_1 = otherMesh.triangles[normalTriangleIndex + 1];
            int vertexIndexC_1 = otherMesh.triangles[normalTriangleIndex + 2];

            foreach (var item in new int[] { vertexIndexA_1, vertexIndexB_1, vertexIndexC_1 })
            {
                (int x, int z) = otherMesh.GetXZ(item);

                if ((otherDireciton == 0 || otherDireciton == 2) && (x == 0 || x == otherMesh.sizeX - 1))
                {
                    // Horizontal
                    otherSeamNormals[z] += surfaceNormal;
                }
                else if ((otherDireciton == 1 || otherDireciton == 3) && (z == 0 || z == otherMesh.sizeZ - 1))
                {
                    // Vertical
                    otherSeamNormals[x] += surfaceNormal;

                }
            }
        }

        // TODO: fix so we can have different size mesh output of terrain graph
        UpdateNormals(ref normals, otherSeamNormals, direction);
    }

    public static void UpdateSeamColormap(ref Color[] colormap, Color[] otherColormap, int direction)
    {
        int otherSeamLength = Mathf.RoundToInt(Mathf.Sqrt(otherColormap.Length));
        int otherDirection = (direction + 2) % 4;

        Color[] seamColors = new Color[otherSeamLength];

        for (int i = 0; i < otherSeamLength; i++)
        {
            int otherIndex = SeamToArrayIndex(otherSeamLength, otherDirection, i);

            seamColors[i] = otherColormap[otherIndex];
        }

        UpdateColors(ref colormap, seamColors, direction);

        // TODO: Implement this better solution, but where colormap can be of different size than otherColormap
        //int seamLength = Mathf.RoundToInt(Mathf.Sqrt(colormap.Length));
        //int otherDirection = (direction + 2) % 4;

        //for (int i = 0; i < seamLength; i++)
        //{
        //    int index = SeamIndex(seamLength, direction, i);
        //    int otherIndex = SeamIndex(seamLength, otherDirection, i);

        //    Color otherColor = otherColormap[otherIndex];

        //    if (i == 0 || i == seamLength - 1)
        //    {
        //        colormap[index] = colormap[index] + otherColor / 4;
        //    }
        //    else
        //    {
        //        colormap[index] = colormap[i] + otherColor / 2;
        //    }
        //}
    }

    public static int SeamToArrayIndex(int seamCount, int direction, int i)
    {
        switch (direction)
        {
            case 0:
                // Right
                return seamCount * i + (seamCount - 1);
            case 1:
                // Up
                return seamCount * (seamCount - 1) + i;
            case 2:
                // Left
                return i * seamCount;
            case 3:
                // Down
                return i;
        }

        throw new System.Exception("Direction must be greater or equal 0 and less then 4");
    }

    //public static bool IsOnSeam(int seamCount, int direction, int i)
    //{
    //    switch (direction)
    //    {
    //        case 0:
    //            // Right
    //            return ((float)i - (float)seamCount) / (float)seamCount == 0;
    //        case 1:
    //            // Up
    //            return seamCount * (seamCount - 1) + i;
    //        case 2:
    //            // Left
    //            return i * seamCount;
    //        case 3:
    //            // Down
    //            return i;
    //    }
    //}
}