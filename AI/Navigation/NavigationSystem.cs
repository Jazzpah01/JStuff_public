using JStuff.AI.Pathfinding;
using JStuff.AI.Steering;
using JStuff.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationSystem : MonoBehaviour {
    public static NavigationSystem instance;

    private NavRegion graphRegion;
    private List<ISteeringAgent> agents;
    private List<ISteeringAgent> agentTracker;
    private Stack<ISteeringAgent> newAgents;
    private Stack<ISteeringAgent> destroyedAgents;

    [Header("Settings")]
    public UpAxis up = UpAxis.Y;
    public float stationaryWeight = 10;
    public float updateInterval = 0.5f;
    [Range(0f, 1f)]
    public float agentMaxStationary = 0.9f;
    [Range(0f, 1f)]
    public float stationaryPathingThreshold = 0.5f;
    [Min(0.001f)]
    public float standardAgentSize = 0.5f;
    [Min(0.001f)]
    public float density = 0.5f;
    [Min(0)]
    public float pathfindingCooldown = 0.5f;

    [Header("Behaviors")]
    public SteeringPathSO steeringPath;

    [Header("Debugging")]
    public bool debugging = false;

    private List<WeightedAgent> weightedAgents;

    private float updateTime;

    public List<Vector2> pathVisualization;

    public struct WeightedAgent : ICircleCollider
    {
        public Vector2 position;
        public float radius;
        public float weight;

        public WeightedAgent(ISteeringAgent agent)
        {
            position = agent.Position;
            radius = agent.Radius;
            weight = agent.Stationary;
        }

        public void Update(ISteeringAgent agent)
        {
            position = agent.Position;
            radius = agent.Radius;
            weight = agent.Stationary;
        }

        public Vector2 Position => position;

        public float Radius => radius;

        public Vector2 MinPosition => position - new Vector2(radius, radius);

        public Vector2 MaxPosition => position + new Vector2(radius, radius);
    }

    private void Awake()
    {
        instance = this;
        agents = new List<ISteeringAgent>();
        weightedAgents = new List<WeightedAgent>();
        newAgents = new Stack<ISteeringAgent>();
        destroyedAgents = new Stack<ISteeringAgent>();
        pathVisualization = new List<Vector2>();

        agentTracker = new List<ISteeringAgent>();

        if (!Application.isEditor)
            debugging = false;
    }

    private void Start()
    {
        updateTime = updateInterval;
    }

    private void Update()
    {
        if (graphRegion == null)
            return;

        updateTime -= Time.deltaTime;
        if (updateTime <= 0)
        {
            updateTime = updateInterval;
            UpdateWeights();
        }
    }

    public void QueuePath(SteeringAgent agent, Vector2 goal)
    {
        int index = agents.IndexOf(agent);

        GridGraph graph = (GridGraph)graphRegion.GetGraph(agent.Radius, weightedAgents[index]);
        int s = graph.GetClosestNode(agent.Position);
        int t = graph.GetClosestNode(goal);

        PathfindingJob job = new PathfindingJob();
        job.s = s;
        job.t = t;
        job.graph = graph;
        job.goal = goal;
        job.agent = agent;

        JobManagerComponent.instance.manager.AddJob(agent, CalculatePath, job);
    }

    public void AddGraphRegion(NavRegion graph)
    {
        this.graphRegion = graph;
        graph.SetupGraph(standardAgentSize);
    }

    public void AddAgent(ISteeringAgent agent)
    {
        newAgents.Push(agent);
        agentTracker.Add(agent);
    }

    public void RemoveAgent(ISteeringAgent agent)
    {
        //destroyedAgents.Push(agent);
        if (!Application.isPlaying)
            return;

        if (graphRegion != null)
        {
            graphRegion.UpdateWeights(agent, -weightedAgents[weightedAgents.Count - 1].weight);
            weightedAgents.RemoveAt(agents.IndexOf(agent));
        }

        agents.Remove(agent);
        agentTracker.Remove(agent);
    }

    public void UpdateWeights()
    {
        // Remove destroyed agents
        while (destroyedAgents.Count > 0)
        {
            ISteeringAgent agent = destroyedAgents.Pop();

            if (graphRegion != null)
                graphRegion.UpdateWeights(agent, -weightedAgents[weightedAgents.Count - 1].weight);
            weightedAgents.RemoveAt(agents.IndexOf(agent));
            agents.Remove(agent);
        }

        if (graphRegion != null)
        {
            // Update current agents
            for (int i = 0; i < agents.Count; i++)
            {
                float m = weightedAgents[i].weight;
                graphRegion.UpdateWeights(weightedAgents[i], -weightedAgents[i].weight);
                weightedAgents[i] = new WeightedAgent(agents[i]);
                float n = weightedAgents[i].weight;
                graphRegion.UpdateWeights(weightedAgents[i], weightedAgents[i].weight);

                //if (m != n)
                //    Debug.Log($"Updating weight. Old: {m}. New: {n}.");
            }
        }
        
        // Add new agents
        while(newAgents.Count > 0)
        {
            ISteeringAgent agent = newAgents.Pop();

            agents.Add(agent);
            weightedAgents.Add(new WeightedAgent(agent));
            if (graphRegion != null)
                graphRegion.UpdateWeights(agent, weightedAgents[weightedAgents.Count - 1].weight);
        }
    }

    private object CalculatePath(object parameters)
    {
        PathfindingJob job = (PathfindingJob)parameters;
        PathfindingResult retval = new PathfindingResult();
        retval.goal = job.goal;

        retval.path = ShortestPath.AStar<Vector2>(job.graph, job.s, job.t, job.graph.DiagonalDistance);

        return retval;
    }

    public void VisualizePath(IList<Vector2> path)
    {
        pathVisualization = new List<Vector2>();
        for (int i = 0; i < path.Count; i++)
        {
            pathVisualization.Add(path[i]);
        }
    }

    public IList<ISteeringAgent> GetAgents()
    {
        return agents;
    }

    public ISteeringAgent GetClosestAgent(Vector2 v)
    {
        float d = float.MaxValue;
        ISteeringAgent retval = null;

        foreach (ISteeringAgent agent in agents)
        {
            float nd = Vector2.Distance(v, agent.Position);
            if (nd < d)
            {
                d = nd;
                retval = agent;
            }
        }

        return retval;
    }

    /// <summary>
    /// Get world position from navigation position. Only usable in runtime.
    /// </summary>
    /// <param name="navPosition"></param>
    /// <returns></returns>
    public Vector3 NavigationToWorld(Vector2 navPosition)
    {
        if (up == UpAxis.Y)
        {
            return new Vector3(navPosition.x, 0, navPosition.y);
        } else
        {
            return new Vector3(navPosition.x, navPosition.y, 0);
        }
    }

    /// <summary>
    /// Get navigation position from world position. Only usable in runtime.
    /// </summary>
    /// <param name="worldPosition"></param>
    /// <returns></returns>
    public Vector2 WorldToNavigation(Vector3 worldPosition)
    {
        if (up == UpAxis.Y)
        {
            return new Vector2(worldPosition.x, worldPosition.z);
        }
        else
        {
            return new Vector2(worldPosition.x, worldPosition.y);
        }
    }
}