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

                //Debug.Log($"Pos: {(x, z)}. LOD: {LOD}. otherSeamIndex: {i * LOD}. This seam length: {thisNormalSeamCount}. Other seam length: {otherNormalSeamCount}.");

                normals[meshIndex] += otherSeamNormals[i * LOD];

                //while ((j - 1) * otherScale < (i - 1) * thisScale && Mathf.Abs((j - 1) * otherScale - (i - 1) * thisScale) > 0.000001f)
                //while (j * otherScale < i * thisScale && 
                //    Mathf.Abs(j * otherScale - i * thisScale) > 0.00000001f)
                //{
                //    j++;
                //}

                //int meshIndex = SeamToArrayIndex(thisNormalSeamCount, direction, i);

                //normals[meshIndex] += otherSeamNormals[j];
            }
        }
        //else if (thisNormalSeamCount > otherNormalSeamCount)
        //{
        //    // Needs interpolation
        //    int j = 0;
        //    for (int i = 0; i < thisNormalSeamCount; i++)
        //    {
        //        while ((j - 1) * otherScale < (i - 1) * thisScale && Mathf.Abs((j - 1) * otherScale - (i - 1) * thisScale) > 0.0001f)
        //        {
        //            j++;
        //        }

        //        int meshIndex = SeamToArrayIndex(thisNormalSeamCount, direction, i);

        //        if (Mathf.Abs((j - 1f) * otherScale - (i - 1f) * thisScale) < 0.0001f)
        //        {
        //            // same index
        //            normals[meshIndex] += otherSeamNormals[j];
        //        }
        //        else
        //        {
        //            float t = ((i - 1f) * thisScale).Remap((j - 2f) * otherScale, (j - 1f) * otherScale, 0f, 1f);
        //            normals[meshIndex] += otherSeamNormals[j - 1] * (1 - t) + otherSeamNormals[j] * t;
        //        }
        //    }
        //}
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

    static (int, int)[] directionThingie = new (int, int)[]
        {
            (1, 0),
            (0, 1),
            (-1, 0),
            (0, -1),
        };

    public static void UpdateSeamNormals(MeshData otherMesh, ref Vector3[] normals, int direction)
    {
        //for (int i = 0; i < normals.Length; i++)
        //{
        //    normals[i] = Vector3.zero;
        //}

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