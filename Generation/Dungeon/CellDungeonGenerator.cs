using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CellDungeonGenerator
{
    public static GameObject[,] GenerateDungeon(CellDungeonTemplate template, CellGrid grid, Vector3 offset, Vector3 stride,
        Action<CellGrid> Connect, Action<GameObject> GOTransform)
    {
        GameObject[,] retval = new GameObject[grid.Length0, grid.Length1];

        // Connect CellGrid
        Connect(grid);

        // Fill using connections
        retval = Fill(template, grid, offset, stride);

        // Transform GameObjects
        TransformGameObjects(retval, GOTransform);

        return retval;
    }

    public static GameObject[,] GenerateDungeon(CellDungeonTemplate template, CellGrid grid, Vector3 offset, Vector3 stride,
        Action<CellGrid> Connect)
    {
        GameObject[,] retval = new GameObject[grid.Length0, grid.Length1];

        // Connect CellGrid
        Connect(grid);

        // Fill using connections
        retval = Fill(template, grid, offset, stride);

        return retval;
    }

    public static GameObject[,] GenerateDungeon(CellDungeonTemplate template, CellGrid grid, Vector3 offset, Vector3 stride,
        Action<GameObject> GOTransform)
    {
        GameObject[,] retval = new GameObject[grid.Length0, grid.Length1];

        // Fill using connections
        retval = Fill(template, grid, offset, stride);

        // Transform GameObjects
        TransformGameObjects(retval, GOTransform);

        return retval;
    }

    public static GameObject[,] GenerateDungeon(CellDungeonTemplate template, CellGrid grid, Vector3 offset, Vector3 stride)
    {
        // Fill using connections
        return Fill(template, grid, offset, stride);
    }

    private static GameObject[,] Fill(CellDungeonTemplate template, CellGrid grid, Vector3 offset, Vector3 stride)
    {
        GameObject[,] retval = new GameObject[grid.Length0, grid.Length1];

        for (int i = 0; i < grid.TotalLength; i++)
        {
            (int x, int y) = grid[i];
            Connections con = grid.GetConnections(i);

            GameObject go = MonoBehaviour.Instantiate(template.GetPrefab(con));

            go.transform.position = offset + new Vector3(stride.x * x, stride.y * y, stride.z * y);

            retval[x, y] = go;
        }

        return retval;
    }

    private static void TransformGameObjects(GameObject[,] gameObjects, Action<GameObject> function)
    {
        for (int y = 0; y < gameObjects.GetLength(1); y++)
        {
            for (int x = 0; x < gameObjects.GetLength(0); x++)
            {
                function(gameObjects[x, y]);
            }
        }
    }
}