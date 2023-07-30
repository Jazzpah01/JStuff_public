using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.Utilities;
using JStuff.Generation;

public class Seam
{
    public static void UpdateNormals(ref Vector3[] normals, Vector3[] otherSeamNormals, int direction)
    {
        int thisNormalSeamCount = Mathf.RoundToInt(Mathf.Sqrt(normals.Length));
        int otherNormalSeamCount = otherSeamNormals.Length;

        int thisScale = 1 / (thisNormalSeamCount - 1);
        int otherScale = 1 / (otherNormalSeamCount - 1);

        //return;

        if (thisNormalSeamCount == otherNormalSeamCount)
        {
            for (int i = 0; i < thisNormalSeamCount; i++)
            {
                int meshIndex = SeamToArrayIndex(thisNormalSeamCount, direction, i);

                normals[meshIndex] += otherSeamNormals[i];
            }
        }
        if (thisNormalSeamCount < otherNormalSeamCount)
        {
            // No interpolation
            int j = 0;

            int LOD = (otherNormalSeamCount - 1) / (thisNormalSeamCount - 1);
            for (int i = 0; i < thisNormalSeamCount; i++)
            {
                int meshIndex = SeamToArrayIndex(thisNormalSeamCount, direction, i);

                (int x, int z) = (i % thisNormalSeamCount, i / thisNormalSeamCount);

                normals[meshIndex] += otherSeamNormals[i * LOD];
            }
        }
    }

    public static void UpdateColors(ref Color[] colormap, Color[] seamColormap, int direction)
    {
        if (direction < 0 || direction >= 4)
        {
            throw new System.Exception();
        }

        int thisNormalSeamCount = Mathf.RoundToInt(Mathf.Sqrt(colormap.Length));
        int otherNormalSeamCount = seamColormap.Length;

        float thisScale = 1f / (thisNormalSeamCount - 1f);
        float otherScale = 1f / (otherNormalSeamCount - 1f);

        if (thisNormalSeamCount == otherNormalSeamCount)
        {
            for (int i = 0; i < thisNormalSeamCount; i++)
            {
                int meshIndex = SeamToArrayIndex(thisNormalSeamCount, direction, i);

                colormap[meshIndex] = (colormap[meshIndex] + seamColormap[i]) / 2f;
            }
        }
        else if (thisNormalSeamCount < otherNormalSeamCount)
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

    public static void UpdateSeamNormals(MeshData otherMesh, ref Vector3[] normals, int direction)
    {
        int otherDireciton = (direction + 2) % 4;

        int otherTriangleCount = otherMesh.triangles.Length / 3;

        //int otherSeamSquareCount = Mathf.RoundToInt(Mathf.Sqrt(otherTriangleCount / 2));
        int otherSeamSquareCount = Mathf.RoundToInt(Mathf.Sqrt(normals.Length));

        int LOD = (otherMesh.sizeX - 1) / (otherSeamSquareCount - 1);

        // Get seam normals of other
        for (int i = 0; i < otherSeamSquareCount - 1; i++)
        {
            (int x0, int z0) = (0, 0);
            if (otherDireciton == 0)
            {
                (x0, z0) = (LOD * (otherSeamSquareCount - 2), LOD * i);
            }
            else if (otherDireciton == 1)
            {
                (x0, z0) = (LOD * i, LOD * (otherSeamSquareCount - 2));
            }
            else if (otherDireciton == 2)
            {
                (x0, z0) = (0, LOD * i);
            }
            else if (otherDireciton == 3)
            {
                (x0, z0) = (LOD * i, 0);
            }
            
            int vertexIndexA_0 = otherMesh.GetIndex(x0, z0);
            int vertexIndexB_0 = otherMesh.GetIndex(x0, z0 + LOD);
            int vertexIndexC_0 = otherMesh.GetIndex(x0 + LOD, z0 + LOD);

            Vector3 surfaceNormal = otherMesh.SurfaceNormal(vertexIndexA_0, vertexIndexB_0, vertexIndexC_0);

            foreach (var item in new int[] { vertexIndexA_0, vertexIndexB_0, vertexIndexC_0 })
            {
                (int x, int z) = otherMesh.GetXZ(item);

                if ((otherDireciton == 0 || otherDireciton == 2) && (x == 0 || x == otherMesh.sizeX - 1))
                {
                    // Horizontal
                    int normalsIndex = SeamToArrayIndex(otherSeamSquareCount, direction, z / LOD);
                    normals[normalsIndex] += surfaceNormal;
                }
                else if ((otherDireciton == 1 || otherDireciton == 3) && (z == 0 || z == otherMesh.sizeZ - 1))
                {
                    // Vertical
                    int normalsIndex = SeamToArrayIndex(otherSeamSquareCount, direction, x / LOD);
                    normals[normalsIndex] += surfaceNormal;

                }
            }

            (int x1, int z1) = (x0, z0);
            int vertexIndexA_1 = otherMesh.GetIndex(x1, z1);
            int vertexIndexB_1 = otherMesh.GetIndex(x1 + LOD, z1 + LOD);
            int vertexIndexC_1 = otherMesh.GetIndex(x1 + LOD, z1);

            surfaceNormal = otherMesh.SurfaceNormal(vertexIndexA_1, vertexIndexB_1, vertexIndexC_1);

            foreach (var item in new int[] { vertexIndexA_1, vertexIndexB_1, vertexIndexC_1 })
            {
                (int x, int z) = otherMesh.GetXZ(item);

                if ((otherDireciton == 0 || otherDireciton == 2) && (x == 0 || x == otherMesh.sizeX - 1))
                {
                    // Horizontal
                    int normalsIndex = SeamToArrayIndex(otherSeamSquareCount, direction, z / LOD);
                    normals[normalsIndex] += surfaceNormal;
                }
                else if ((otherDireciton == 1 || otherDireciton == 3) && (z == 0 || z == otherMesh.sizeZ - 1))
                {
                    // Vertical
                    int normalsIndex = SeamToArrayIndex(otherSeamSquareCount, direction, x / LOD);
                    normals[normalsIndex] += surfaceNormal;

                }
            }
        }
    }

    public static void UpdateSeamColormap(ref Color[] colormap, Color[] otherColormap, int direction)
    {
        int thisSeamLength = Mathf.RoundToInt(Mathf.Sqrt(colormap.Length));
        int otherSeamLength = Mathf.RoundToInt(Mathf.Sqrt(otherColormap.Length));
        int otherDirection = (direction + 2) % 4;
        int LOD = (otherSeamLength - 1) / (thisSeamLength - 1);

        for (int i = 0; i < thisSeamLength; i++)
        {
            int otherIndex = SeamToArrayIndex(otherSeamLength, otherDirection, i * LOD);
            int thisIndex = SeamToArrayIndex(thisSeamLength, direction, i);

            colormap[thisIndex] = (colormap[thisIndex] + otherColormap[otherIndex]) / 2;
        }
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

    public static void ResetExtrusion(MeshData target, MeshData original)
    {
        int LOD = (original.sizeX - 1) / (target.sizeX - 1);

        foreach (var direction in new int[] { 0, 1, 2, 3 })
        {
            for (int i = 0; i < target.sizeX; i++)
            {
                int targetIndex = SeamToArrayIndex(target.sizeX, direction, i);
                int originalIndex = SeamToArrayIndex(original.sizeX, direction, i * LOD);

                target.vertices[targetIndex] = original.vertices[originalIndex];
            }
        }
    }

    public static void ResetExtrusion(Vector3[] target, Vector3[] original)
    {
        int targetSeamSize = (int)Mathf.Sqrt(target.Length);
        int originalSeamSize = (int)Mathf.Sqrt(original.Length);

        int LOD = (originalSeamSize - 1) / (targetSeamSize - 1);

        foreach (var direction in new int[] { 0, 1, 2, 3 })
        {
            for (int i = 0; i < targetSeamSize; i++)
            {
                int targetIndex = SeamToArrayIndex(targetSeamSize, direction, i);
                int originalIndex = SeamToArrayIndex(originalSeamSize, direction, i * LOD);

                target[targetIndex] = original[originalIndex];
            }
        }
    }

    public static void ExtrudeEdgeVertices(MeshData target, int LOD, int direction)
    {
        int j = 0;
        for (int i = 0; i < target.sizeX; i++)
        {
            if (i % LOD != 0)
            {
                int targetIndex = SeamToArrayIndex(target.sizeX, direction, i);
                int v0Index = SeamToArrayIndex(target.sizeX, direction, j);
                int v1Index = SeamToArrayIndex(target.sizeX, direction, j + LOD);

                float t = ((float)i - (float)j) / (float)LOD;
                float v0 = target.vertices[v0Index].y;
                float v1 = target.vertices[v1Index].y;

                target.vertices[targetIndex].y = Mathf.Lerp(v0, v1, t);
            } else
            {
                j = i;
            }
        }
    }

    //public static void ExtrudeEdgeVertices(MeshData target, float amount)
    //{
    //    foreach (var direction in new int[] { 0, 1, 2, 3 })
    //    {
    //        for (int i = 0; i < target.sizeX; i++)
    //        {
    //            int targetIndex = SeamToArrayIndex(target.sizeX, direction, i);

    //            target.vertices[targetIndex] += new Vector3(0, -amount, 0);
    //        }
    //    }
    //}

    //public static void ExtrudeEdgeVertices(MeshData target, float amount, int direction)
    //{
    //    for (int i = 0; i < target.sizeX; i++)
    //    {
    //        int targetIndex = SeamToArrayIndex(target.sizeX, direction, i);

    //        target.vertices[targetIndex] += new Vector3(0, -amount, 0);
    //    }
    //}

    //public static void ExtrudeCornerVertices(MeshData target, float amount, int direction, int otherDirection)
    //{
    //    if ((direction + 2) % 4 == otherDirection || direction == otherDirection)
    //        throw new System.Exception();

    //    int x = 0;
    //    int y = 0;

    //    if (direction == 0 || otherDirection == 0)
    //        x = target.sizeX - 1;
    //    if (direction == 1 || otherDirection == 1)
    //        y = target.sizeZ - 1;

    //    target.vertices[x + y * target.sizeX] += new Vector3(0, -amount, 0);
    //}
}