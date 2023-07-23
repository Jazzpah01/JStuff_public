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

    public Seam(Vector3[] unormalizedMeshNormals, Color[] colormap, int direction)
    {
        Length = (int) Mathf.Sqrt(unormalizedMeshNormals.Length);
        seamColors = new Color[Length];
        unormalized_normals = new Vector3[Length];
        positions = new float[Length];

        if (direction == 0)
        {
            // Right
            for (int i = 0; i < Length; i++)
            {
                int meshIndex = i * Length + Length - 1;

                unormalized_normals[i] = unormalizedMeshNormals[meshIndex];
                seamColors[i] = colormap[meshIndex];
            }
        } else if (direction == 1)
        {
            // Up
            for (int i = 0; i < Length; i++)
            {
                int meshIndex = i + Length * Length - Length;

                unormalized_normals[i] = unormalizedMeshNormals[meshIndex];
                seamColors[i] = colormap[meshIndex];
            }
        } else if (direction == 2)
        {
            // Left
            for (int i = 0; i < Length; i++)
            {
                int meshIndex = i * Length;

                unormalized_normals[i] = unormalizedMeshNormals[meshIndex];
                seamColors[i] = colormap[meshIndex];
            }
        } else if (direction == 3)
        {
            // Down
            for (int i = 0; i < Length; i++)
            {
                int meshIndex = i;

                unormalized_normals[i] = unormalizedMeshNormals[meshIndex];
                seamColors[i] = colormap[meshIndex];
            }
        }
    }

    public Seam(Color[] seamColors, Vector3[] unormalized_normals, float[] positions)
    {
        if (seamColors == null || unormalized_normals == null || positions == null ||
            seamColors.Length != unormalized_normals.Length || seamColors.Length != positions.Length)
            throw new System.Exception();

        this.seamColors = seamColors;
        this.unormalized_normals = unormalized_normals;
        this.positions = positions;

        this.Length = positions.Length;
    }

    public bool SharedAmount(Seam other)
    {
        return other.positions.Length == positions.Length;
    }

    public static void UpdateNormals(ref Vector3[] normals, Vector3[] seamNormals, int direction)
    {
        int sideCount = (int)Mathf.Sqrt(normals.Length);

        if (direction == 0)
        {
            // Right
            for (int i = 0; i < sideCount; i++)
            {
                int meshIndex = i * sideCount + sideCount - 1;

                normals[meshIndex] = seamNormals[i];
            }
        }
        else if (direction == 1)
        {
            // Up
            for (int i = 0; i < sideCount; i++)
            {
                int meshIndex = i + sideCount * sideCount - sideCount;

                normals[meshIndex] = seamNormals[i];
            }
        }
        else if (direction == 2)
        {
            // Left
            for (int i = 0; i < sideCount; i++)
            {
                int meshIndex = i * sideCount;

                normals[meshIndex] = seamNormals[i];
            }
        }
        else if (direction == 3)
        {
            // Down
            for (int i = 0; i < sideCount; i++)
            {
                int meshIndex = i;

                normals[meshIndex] = seamNormals[i];
            }
        }
    }

    public static void UpdateColors(ref Color[] colormap, Color[] seamColormap, int direction)
    {
        if (direction < 0 || direction >= 4)
        {
            throw new System.Exception();
        }

        int sideCount = (int)Mathf.Sqrt(colormap.Length);

        if (direction == 0)
        {
            // Right
            for (int i = 0; i < sideCount; i++)
            {
                int meshIndex = i * sideCount + sideCount - 1;

                colormap[meshIndex] = seamColormap[i];
            }
        }
        else if (direction == 1)
        {
            // Up
            for (int i = 0; i < sideCount; i++)
            {
                int meshIndex = i + sideCount * sideCount - sideCount;

                colormap[meshIndex] = seamColormap[i];
            }
        }
        else if (direction == 2)
        {
            // Left
            for (int i = 0; i < sideCount; i++)
            {
                int meshIndex = i * sideCount;

                colormap[meshIndex] = seamColormap[i];
            }
        }
        else if (direction == 3)
        {
            // Down
            for (int i = 0; i < sideCount; i++)
            {
                int meshIndex = i;

                colormap[meshIndex] = seamColormap[i];
            }
        }
    }

    public static void UpdateNormalsAndColors(ref Vector3[] normals, ref Color[] colormap, Vector3[] seamNormals, Color[] seamColormap, int direction)
    {
        if (normals.Length != colormap.Length)
        {
            throw new System.Exception($"Length of normals ({normals.Length}) is not equal length of colormap ({colormap.Length}).");
        }
        if (normals.Length != seamNormals.Length * seamColormap.Length)
        {
            throw new System.Exception($"Length of normals ({normals.Length}) is not equal length of seam normals squared ({seamNormals.Length * seamNormals.Length}).");
        }
        if (seamNormals.Length != seamColormap.Length)
        {
            throw new System.Exception();
        }
        if (direction < 0 || direction >= 4)
        {
            throw new System.Exception();
        }

        int sideCount = (int)Mathf.Sqrt(normals.Length);

        if (direction == 0)
        {
            // Right
            for (int i = 0; i < sideCount; i++)
            {
                int meshIndex = i * sideCount + sideCount - 1;

                normals[meshIndex] = seamNormals[i];
                colormap[meshIndex] = seamColormap[i];
            }
        }
        else if (direction == 1)
        {
            // Up
            for (int i = 0; i < sideCount; i++)
            {
                int meshIndex = i + sideCount * sideCount - sideCount;

                normals[meshIndex] = seamNormals[i];
                colormap[meshIndex] = seamColormap[i];
            }
        }
        else if (direction == 2)
        {
            // Left
            for (int i = 0; i < sideCount; i++)
            {
                int meshIndex = i * sideCount;

                normals[meshIndex] = seamNormals[i];
                colormap[meshIndex] = seamColormap[i];
            }
        }
        else if (direction == 3)
        {
            // Down
            for (int i = 0; i < sideCount; i++)
            {
                int meshIndex = i;

                normals[meshIndex] = seamNormals[i];
                colormap[meshIndex] = seamColormap[i];
            }
        }
    }

    public void CombineSeams(Seam other, ref Vector3[] newSeamNormals, ref Color[] newSeamColors)
    {
        int diff = positions.Length - other.positions.Length;

        if (diff == 0)
        {
            for (int i = 0; i < positions.Length; i++)
            {
                try
                {
                    newSeamNormals[i] = (unormalized_normals[i] + other.unormalized_normals[i]).normalized;
                    newSeamColors[i] = Color.Lerp(seamColors[i], other.seamColors[i], 0.5f);
                } catch
                {
                    Debug.Log($"Not wotking!!! Length of seam is {this.Length}. Length of normals is: {newSeamNormals.Length}");
                }
                
            }
        }
        else if (positions.Length < other.positions.Length)
        {
            // No interpolation
            int j = 0;
            for (int i = 0; i < positions.Length; i++)
            {
                while (other.positions[j] < positions[i])
                {
                    j++;
                }

                newSeamNormals[i] = (unormalized_normals[i] + other.unormalized_normals[j]).normalized;
                newSeamColors[i] = Color.Lerp(seamColors[i], other.seamColors[j], 0.5f);
            }
        }
        else if (positions.Length > other.positions.Length)
        {
            // Needs interpolation
            int j = 0;
            for (int i = 0; i < positions.Length; i++)
            {
                while (other.positions[j] < positions[i])
                {
                    j++;
                }

                if (other.positions[j] == positions[i])
                {
                    newSeamNormals[i] = (unormalized_normals[i] + other.unormalized_normals[j]).normalized;
                    newSeamColors[i] = Color.Lerp(seamColors[i], other.seamColors[j], 0.5f);
                }
                else
                {
                    // position i is between position j and position j - 1
                    float t = (positions[i] - other.positions[j - 1]).Remap(0, other.positions[j] - other.positions[j - 1], 0, 1);
                    Color otherColor = Color.Lerp(other.seamColors[j - 1], other.seamColors[j], t);
                    Vector3 otherUNormal = other.unormalized_normals[j - 1] * (1 - t) + other.unormalized_normals[j] * t;
                    newSeamNormals[i] = (unormalized_normals[i] + otherUNormal).normalized;
                    newSeamColors[i] = Color.Lerp(seamColors[i], otherColor, 0.5f);
                }
            }
        }
    }

    public Vector3[] CombineSeamsNormals(Seam other)
    {
        int diff = positions.Length - other.positions.Length;

        Vector3[] retval = new Vector3[positions.Length];

        if (diff == 0)
        {
            for (int i = 0; i < positions.Length; i++)
            {
                retval[i] = (unormalized_normals[i] + other.unormalized_normals[i]).normalized;
            }
        } else if (positions.Length < other.positions.Length)
        {
            // No interpolation
            int j = 0;
            for (int i = 0; i < positions.Length; i++)
            {
                while (other.positions[j] < positions[i])
                {
                    j++;
                }

                retval[i] = (unormalized_normals[i] + other.unormalized_normals[j]).normalized;
            }
        } else if (positions.Length > other.positions.Length)
        {
            // Needs interpolation
            int j = 0;
            for (int i = 0; i < positions.Length; i++)
            {
                while (other.positions[j] < positions[i])
                {
                    j++;
                }

                if (other.positions[j] == positions[i])
                {
                    retval[i] = (unormalized_normals[i] + other.unormalized_normals[j]).normalized;
                } else
                {
                    // position i is between position j and position j - 1
                    float t = (positions[i] - other.positions[j - 1]).Remap(0, other.positions[j] - other.positions[j - 1], 0, 1);
                    Vector3 otherUNormal = other.unormalized_normals[j - 1] * (1 - t) + other.unormalized_normals[j] * t;
                    retval[i] = (unormalized_normals[i] + otherUNormal).normalized;
                }
            }
        }

        return retval;
    }

    public void UpdateSeamNormals(MeshData thisMesh, MeshData otherMesh, ref Vector3[] normals, int direction)
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
        int seamLength = Mathf.RoundToInt(Mathf.Sqrt(otherColormap.Length));
        int otherDirection = (direction + 2) % 4;

        Color[] seamColors = new Color[seamLength];

        for (int i = 0; i < seamLength; i++)
        {
            int otherIndex = SeamToArrayIndex(seamLength, otherDirection, i);

            seamColors[i] = otherColormap[otherIndex];
        }

        

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
                return seamCount * i + seamCount;
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