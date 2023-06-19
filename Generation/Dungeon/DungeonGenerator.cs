using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DungeonGenerator : MonoBehaviour
{
    public static DungeonGenerator instance;

    public Vector3 offset;
    public Vector3 stride;

    public CellDungeonTemplate template;

    public int xSize;
    public int ySize;

    private int seed;

    private System.Random rng;

    protected virtual void Awake()
    {
        instance = this;
    }

    protected virtual void Start()
    {
        int seed = UnityEngine.Random.Range(0, int.MaxValue);
        Generate(seed);
    }

    public virtual void Generate(int seed)
    {
        this.seed = seed;

        this.rng = new System.Random(seed);

        CellDungeonGenerator.GenerateDungeon(template, new CellGrid(xSize, ySize), offset, stride, SimpleConnect);
    }

    private void SimpleConnect(CellGrid grid)
    {
        int x = 0;
        int y = 0;

        while(x < grid.Length0 - 1 || y < grid.Length1 - 1)
        {
            if (y == grid.Length1 - 1 || x != grid.Length0 - 1 && rng.NextDouble() < 0.5)
            {
                grid.AddEdge(grid[x + 1, y], grid[x, y]);
                x++;
            } else
            {
                grid.AddEdge(grid[x, y + 1], grid[x, y]);
                y++;
            }
        }

        Debug.Log($"({x},{y})");
        Debug.Log($"Lengths: ({grid.Length0},{grid.Length1})");
    }
}
