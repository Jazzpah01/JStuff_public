using JStuff.AI.Pathfinding;
using JStuff.AI.Steering;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Navigation {
    private static IGraph<Vector2> graph;
    private static List<ISteeringAgent> agents;

    public static void GetPath(ISteeringAgent agent, IGraph<Vector2> goal)
    {
        //ShortestPath.AStar<Vector2>(graph, )
    }

    public static void SetupGraph(IGraph<Vector2> graph)
    {
        Navigation.graph = graph;
    }
}