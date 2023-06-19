using JStuff.AI.Pathfinding;
using JStuff.AI.Steering;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PathfindingJob
{
    public ISteeringAgent agent;
    public Vector2 goal;
    public GridGraph graph;
    public int s;
    public int t;
}
