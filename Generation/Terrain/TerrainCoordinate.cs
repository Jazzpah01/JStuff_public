using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct TerrainCoordinate
{
    int x;
    int y;

    public TerrainCoordinate(float chunkSize, Vector3 chunkPosition)
    {
        x = Mathf.RoundToInt(chunkPosition.x / chunkSize);
        y = Mathf.RoundToInt(chunkPosition.z / chunkSize);
    }

    public TerrainCoordinate(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public Vector3 BlockPosition(float chunkSize)
    {
        return new Vector3(x * chunkSize, 0, y * chunkSize);
    }

    public static TerrainCoordinate operator +(TerrainCoordinate a, TerrainCoordinate b)
    {
        return new TerrainCoordinate(a.x + b.x, a.y + b.y);
    }

    public override string ToString()
    {
        return $"({x},{y})";
    }
}