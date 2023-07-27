using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct TerrainCoordinate
{
    public int x;
    public int y;

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

    public static Vector3 operator *(TerrainCoordinate a, float scalar)
    {
        return new Vector3(a.x, 0, a.y) * scalar;
    }

    public static bool operator ==(TerrainCoordinate a, TerrainCoordinate b)
    {
        return a.x == b.x && a.y == b.y;
    }

    public static bool operator !=(TerrainCoordinate a, TerrainCoordinate b)
    {
        return a.x != b.x || a.y != b.y;
    }

    public static TerrainCoordinate[] DirectionCoordinate = new TerrainCoordinate[]
        {
            new TerrainCoordinate(1, 0),
            new TerrainCoordinate(0, 1),
            new TerrainCoordinate(-1, 0),
            new TerrainCoordinate(0, -1),
        };

    public override string ToString()
    {
        return $"({x},{y})";
    }
}