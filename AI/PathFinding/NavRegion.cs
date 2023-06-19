using JStuff.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.AI.Pathfinding;
using JStuff.AI.Steering;

//TODO: make multible graphs for different agent sizes
[RequireComponent(typeof(SpriteRenderer))]
public class NavRegion : Region
{
    public NavigationSystem navigationSystem;
    public float standardAgentSize = 0;
    public float density = 0;

    private GridGraph graph;
    private NavPoint[,] navPoints;
    private float stationaryWeight = 1;
    private Vector2 offset = new Vector2(0,0);

    public struct NavPoint
    {
        public float weight;
        public int obstacleAmount;
    }

    private void Awake()
    {

    }

    private void Start()
    {
        navigationSystem = NavigationSystem.instance;
        navigationSystem.AddGraphRegion(this);

        density = navigationSystem.density;
        standardAgentSize = navigationSystem.standardAgentSize;
    }

    public void SetupGraph(float agentSize)
    {
        stationaryWeight = navigationSystem.stationaryWeight;
        graph = new GridGraph(GetNodes(standardAgentSize));
        navPoints = new NavPoint[graph.Length0, graph.Length1];
    }

    private Vector2 GetOffset()
    {
        if (Range.x < standardAgentSize || Range.y < standardAgentSize)
            throw new System.Exception();

        Vector2 stride = Vector2.one * standardAgentSize / density;

        int xAmount = Mathf.FloorToInt((float)(Range.x - standardAgentSize) / stride.x);
        int yAmount = Mathf.FloorToInt((float)(Range.y - standardAgentSize) / stride.y);

        Vector2 innerRange = (Range - Vector2.one * standardAgentSize - new Vector2(stride.x * (xAmount - 1), stride.y * (yAmount - 1)));
        Vector2 offset = MinPosition + innerRange / 2 + new Vector2(standardAgentSize, standardAgentSize) / 2;
        return offset;
    }

    public Vector2[,] GetNodes(float agentRadius)
    {
        if (Range.x < agentRadius || Range.y < agentRadius)
            return null;

        Vector2 stride = Vector2.one * agentRadius / density;

        int xAmount = Mathf.FloorToInt((float)(Range.x - agentRadius) / stride.x);
        int yAmount = Mathf.FloorToInt((float)(Range.y - agentRadius) / stride.y);

        Vector2 innerRange = (Range - Vector2.one * agentRadius - new Vector2(stride.x * (xAmount-1), stride.y * (yAmount-1)));
        Vector2 offset = MinPosition + innerRange / 2 + new Vector2(agentRadius, agentRadius) / 2;
        this.offset = offset;

        Vector2[,] retval = new Vector2[xAmount, yAmount];

        for (int x = 0; x < xAmount; x++)
        {
            for (int y = 0; y < yAmount; y++)
            {
                retval[x, y] = new Vector2((float)x * stride.x, (float)y * stride.y) + offset;
            }
        }

        return retval;
    }

    public void UpdateWeights(ICircleCollider circleCollider, float weight)
    {
        Vector2 stride = graph.Stride;

        Vector2 norm = circleCollider.Position - MinPosition;
        int closestX = Mathf.RoundToInt(norm.x / stride.x);
        int closestY = Mathf.RoundToInt(norm.y / stride.y);

        int d = Mathf.CeilToInt(navigationSystem.standardAgentSize * 3);

        float agentHalfSize = navigationSystem.standardAgentSize;

        for (int i = closestX - d; i < closestX + d; i++)
        {
            if (i < 0 || i >= navPoints.GetLength(0))
                continue;

            for (int j = closestY - d; j < closestY + d; j++)
            {
                if (j < 0 || j >= navPoints.GetLength(1))
                    continue;

                Vector2 point = new Vector2(i * stride.x, j * stride.y) + offset;

                if (JCollision.Collides(circleCollider.Position, circleCollider.Radius + agentHalfSize, point))
                {
                    // Point is inside
                    navPoints[i, j].weight += weight;
                }

                if (navPoints[i, j].weight < 0)
                {
                    navPoints[i, j].weight = 0;
                    Debug.LogError($"Weights on graph are less than 0: {navPoints[i, j]}");
                }
                    
            }
        }
    }

    public void UpdateObstacles(IAABB obstacle, int amount)
    {
        Vector2 stride = graph.Stride;

        Vector2 norm = obstacle.Position - MinPosition;
        int closestX = Mathf.RoundToInt(norm.x / stride.x);
        int closestY = Mathf.RoundToInt(norm.y / stride.y);

        int d = Mathf.CeilToInt(navigationSystem.standardAgentSize * 3);

        float agentHalfSize = navigationSystem.standardAgentSize;

        for (int i = closestX - d; i < closestX + d; i++)
        {
            if (i < 0 || i >= navPoints.GetLength(0))
                continue;

            for (int j = closestY - d; j < closestY + d; j++)
            {
                if (j < 0 || j >= navPoints.GetLength(1))
                    continue;

                Vector2 point = new Vector2(i * stride.x, j * stride.y) + offset;

                if (JCollision.Collides(obstacle, point))
                {
                    // Point is inside
                    navPoints[i, j].obstacleAmount += 1;
                }

                if (navPoints[i, j].obstacleAmount < 0)
                    throw new System.Exception($"Weights on graph are less than 0: {navPoints[i, j]}");
            }
        }
    }

    public IGraph<Vector2> GetGraph(float agentRedius, params NavigationSystem.WeightedAgent[] excludeAgents)
    {
        foreach (NavigationSystem.WeightedAgent agent in excludeAgents)
        {
            UpdateWeights(agent, -agent.weight);
        }

        GridGraph retval = graph.Clone();

        retval.ConstructEdges<NavPoint>(navPoints, HasEdge, GetWeight, EdgeOptions.UseDiagonals | EdgeOptions.AddDistance);

        foreach (NavigationSystem.WeightedAgent agent in excludeAgents)
        {
            UpdateWeights(agent, agent.weight);
        }

        return retval;
    }

    public string StationaryToString()
    {
        string retval = "";

        float max = float.MinValue;
        float min = float.MaxValue;

        for (int i = 0; i < navPoints.GetLength(0); i++)
        {
            for (int j = 0; j < navPoints.GetLength(1); j++)
            {
                retval += $"({i}, {j}, {navPoints[i, j]})";

                if (navPoints[i, j].weight < min)
                    min = navPoints[i, j].weight;

                if (navPoints[i, j].weight > max)
                    max = navPoints[i, j].weight;
            }
        }

        return $"Stationaries in graph. Max: {max}. Min: {min}. " + retval;
    }

    private float GetWeight(NavPoint a, NavPoint b) => Mathf.Max(a.weight, b.weight) * stationaryWeight;

    private bool HasEdge(NavPoint a, NavPoint b) => a.obstacleAmount == 0 && b.obstacleAmount == 0;
}