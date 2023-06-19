using JStuff.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleAABB : Region, IObstacle, IAABB
{
    public Vector2 Position => MinPosition + Range / 2;//lazzyyy

    public void Initialize(NavRegion navRegion)
    {
        throw new System.NotImplementedException();
    }
}
