using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.Utilities;
using System;

namespace JStuff.AI.Pathfinding
{
    public static class ShortestPath
    {
        public delegate void SetPath(Node[] path);
        public delegate float HeuristicOld(Node v, Node u, object data = null);

        public delegate float Heuristic(int v, int u);


        public static N[] AStar<N>(IGraph<N> graph, int s, int t, Heuristic heuristic, bool invertedRoute = false)
        {
            // Distance f(v) to u is g(v) + h(v). g is shortest path s->v, h is heuristic for v->t.
            // where g is the current shortest distance and h is a heuristic function mapping
            // an approximate distance between some vertex u and t. s is the starting vertex, 
            // u is some vertex, t is the goal vertex.

            // Sanitize input
            if (s == t)
                return null;

            if (!graph.Contains(s))
                return null;

            if (!graph.Contains(t))
                return null;

            // Begin algorithm
            float[] g = new float[graph.Size]; // Current shortest distance to node
            float[] f = new float[graph.Size]; // Current shortest distance + estimated distance to goal
            int[] prev = new int[graph.Size]; // Previous node of index

            for (int i = 0; i < graph.Size; i++)
            {
                g[i] = float.PositiveInfinity;
                f[i] = float.PositiveInfinity;
            }

            prev.Populate<int>(-1);
            g.Populate<float>(Mathf.Infinity);

            List<int> openSet = new List<int>();
            List<int> T = new List<int>();

            openSet.Add(s);
            f[s] = heuristic(s, t);
            g[s] = 0;

            bool goalReached = false;

            while (openSet.Count > 0 || goalReached)
            {
                int v = openSet[0];

                for (int i = 1; i < openSet.Count; i++)
                {
                    if (f[openSet[i]] < f[v])
                    {
                        v = openSet[i];
                    }
                }

                openSet.Remove(v);
                T.Add(v);

                if (v == t)
                {
                    goalReached = true;
                    break;
                }

                foreach (int u in graph.AdjacentNodes(v))
                {
                    float tentative_cost = g[v] + graph.GetWeight(v, u);
                    // TODO: Need to check for excluded vertices
                    if (tentative_cost < g[u] && u != s)
                    {
                        if (!openSet.Contains(u))
                            openSet.Add(u);
                        g[u] = tentative_cost;
                        f[u] = g[u] + heuristic(u, t);
                        prev[u] = v;
                    }
                }
            }

            if (goalReached)
            {
                List<N> route = new List<N>();
                int p = t;
                while (p != s)
                {
                    if (prev[p] == -1)
                    {
                        throw new System.Exception("Error in code. A node doesn't have a previous.");
                    }

                    route.Add(graph[p]);
                    p = prev[p];
                }
                route.Add(graph[s]);

                if (invertedRoute)
                {
                    return route.ToArray();
                } else
                {
                    List<N> otherRoute = new List<N>();
                    for (int i = route.Count-1; i >= 0; i--)
                    {
                        otherRoute.Add(route[i]);
                    }
                    return otherRoute.ToArray();
                }
            }
            else
            {
                return null;
            }
        }

        public static void AddSorted(this List<float> @this, float item)
        {
            if (@this.Count == 0)
            {
                @this.Add(item);
                return;
            }
            if (@this[@this.Count - 1].CompareTo(item) <= 0)
            {
                @this.Add(item);
                return;
            }
            if (@this[0].CompareTo(item) >= 0)
            {
                @this.Insert(0, item);
                return;
            }
            int index = @this.BinarySearch(item);
            if (index < 0)
                index = ~index;
            @this.Insert(index, item);
        }

        //public static Node[] AStar(Graph G, Node s, Node t, HeuristicOld h, HashSet<Node> excludedVertices = null, object hData = null)
        //{
        //    // Distance dist(v) to u is g(s->v) + h(v->t)
        //    // where g is the current shortest distance and h is a heuristic function mapping
        //    // an approximate distance between some vertex u and t. s is the starting vertex, 
        //    // u is some vertex, t is the goal vertex.
        //
        //    // Sanitize input
        //    if (s.Equals(t))
        //    {
        //        throw new System.Exception("Can't find path between two identical vertices.");
        //    }
        //
        //    bool containss = false;
        //    bool containst = false;
        //
        //    foreach(Node v in G.GetVertices())
        //    {
        //        if (v.Equals(s))
        //            containss = true;
        //
        //        if (v.Equals(t))
        //            containst = true;
        //    }
        //
        //    if (!containss)
        //    {
        //        throw new System.Exception("Can't find path when initial vertex s is not contained in the graph.");
        //    }
        //    if (!containst)
        //    {
        //        throw new System.Exception("Can't find path when destination vertex t is not contained in the graph.");
        //    }
        //
        //    // Begin algorithm
        //    Dictionary<Node, float> dist = new Dictionary<Node, float>();
        //    Dictionary<Node, float> rdist = new Dictionary<Node, float>();
        //    Dictionary<Node, Node> prev = new Dictionary<Node, Node>();
        //
        //    foreach (Node v in G.GetVertices())
        //    {
        //        dist.Add(v, float.PositiveInfinity);
        //    }
        //
        //    List<Node> S = new List<Node>();
        //
        //    S.Add(s);
        //    rdist[s] = h(s, t, hData);
        //    dist[s] = 0;
        //
        //    int control = 1000;
        //    while (S.Count > 0 && control > 0)
        //    {
        //        //control--;
        //        //if (control < 1)
        //        //    throw new System.Exception("Infinite loop. :(");
        //
        //        (List<Node> nS, Node v) = ExtractMinimum(S, rdist);
        //        S = nS;
        //
        //        if (v.Equals(t))
        //            break;
        //
        //        foreach (Node u in G.GetNeighbors(v))
        //        {
        //            if ((!prev.ContainsKey(u) && !u.Equals(s)) ||
        //                (S.Contains(u) && dist[v] + G.GetWeight(v, u) < dist[u]))
        //            {
        //                if (!S.Contains(u) && (excludedVertices == null || !excludedVertices.Contains(u)))
        //                    S.Add(u);
        //                dist[u] = dist[v] + G.GetWeight(v, u);
        //                rdist[u] = dist[u] + h(u, t, hData);
        //                prev[u] = v;
        //            }
        //        }
        //    }
        //
        //    if (prev.ContainsKey(t))
        //    {
        //        List<Node> route = new List<Node>();
        //        Node p = t;
        //        control = 1000;
        //        while (prev.ContainsKey(p))
        //        {
        //            route.Add(p);
        //            p = prev[p];
        //            control--;
        //            if (control < 1)
        //                throw new System.Exception("Infinite loop. :(");
        //        }
        //        route.Add(s);
        //        return route.ToArray();
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}
        //
        //
        //public static IEnumerator AStarCoroutine(Graph G, Node s, Node t, SetPath setPath, HeuristicOld h, HashSet<Node> excludedVertices = null, object hData = null)
        //{
        //    // Distance dist(v) to u is g(s->v) + h(v->t)
        //    // where g is the current shortest distance and h is a heuristic function mapping
        //    // an approximate distance between some vertex u and t. s is the starting vertex, 
        //    // u is some vertex, t is the goal vertex.
        //
        //    Debug.Log("AStarCoroutine");
        //
        //    Dictionary<Node, float> dist = new Dictionary<Node, float>();
        //    Dictionary<Node, float> rdist = new Dictionary<Node, float>();
        //    Dictionary<Node, Node> prev = new Dictionary<Node, Node>();
        //
        //    foreach (Node v in G.GetVertices())
        //    {
        //        dist.Add(v, float.PositiveInfinity);
        //    }
        //
        //    List<Node> S = new List<Node>();
        //
        //    S.Add(s);
        //    rdist[s] = h(s, t, hData);
        //    dist[s] = 0;
        //
        //    int control = 1000;
        //    while (S.Count > 0 && control > 0)
        //    {
        //        control--;
        //        if (control < 1)
        //            throw new System.Exception("Infinite loop. :(");
        //
        //        (List<Node> nS, Node v) = ExtractMinimum(S, rdist);
        //        S = nS;
        //
        //        if (v.Equals(t))
        //            break;
        //
        //        foreach (Node u in G.GetNeighbors(v))
        //        {
        //            if ((!prev.ContainsKey(u) && !u.Equals(s)) ||
        //                (S.Contains(u) && dist[v] + G.GetWeight(v, u) < dist[u]))
        //            {
        //                if (!S.Contains(u) && !excludedVertices.Contains(u))
        //                    S.Add(u);
        //                dist[u] = dist[v] + G.GetWeight(v, u);
        //                rdist[u] = dist[u] + h(s, t, hData);
        //                prev[u] = v;
        //            }
        //        }
        //
        //        yield return null;
        //    }
        //
        //    if (prev.ContainsKey(t))
        //    {
        //        List<Node> route = new List<Node>();
        //        Node p = t;
        //        control = 1000;
        //        while (prev.ContainsKey(p))
        //        {
        //            route.Add(p);
        //            p = prev[p];
        //            control--;
        //            if (control < 1)
        //                throw new System.Exception("Infinite loop. :(");
        //        }
        //        route.Add(s);
        //        setPath(route.ToArray());
        //        yield return null;
        //    }
        //    else
        //    {
        //        setPath(null);
        //        yield return null;
        //    }
        //}
        //
        ///// <summary>
        ///// Now with side effects :)
        ///// </summary>
        ///// <param name="S"></param>
        ///// <param name="dist"></param>
        ///// <returns></returns>
        //private static (List<Node>, Node) ExtractMinimum(List<Node> S, Dictionary<Node, float> dist)
        //{
        //    Node smallest = S[0];
        //    float d = dist[smallest];
        //    int control = 1000;
        //    foreach (Node v in S)
        //    {
        //        if (dist[v] < d)
        //        {
        //            d = dist[v];
        //            smallest = v;
        //        }
        //        control--;
        //        if (control < 1)
        //            throw new System.Exception("Infinite loop. :(");
        //    }
        //    S.Remove(smallest);
        //    return (S, smallest);
        //}
        //
        //// http://theory.stanford.edu/~amitp/GameProgramming/Heuristics.html
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="v"></param>
        ///// <param name="u"></param>
        ///// <param name="data">The smallest weight of the graph.</param>
        ///// <returns></returns>
        //public static float ManhattenDistance(Node v, Node u, object data = null)
        //{
        //    return (Mathf.Abs(v.x - u.x) + Mathf.Abs(v.z - u.z)) * (float)data;
        //}
        //
        //public static float DiagonalDistance(Node v, Node u, object data = null)
        //{
        //    float dx = Mathf.Abs(v.x - u.x);
        //    float dy = Mathf.Abs(v.z - u.z);
        //    (float D, float D2) = ((float, float))data;
        //    return D * (dx + dy) + (D2 - 2 * D) * Mathf.Min(dx, dy);
        //}
        //
        //public static float EuclideanDistance(Node v, Node u, object data = null)
        //{
        //    float dx = Mathf.Abs(v.x - u.x);
        //    float dy = Mathf.Abs(v.z - u.z);
        //    return (float)data * Mathf.Sqrt(dx * dx + dy * dy);
        //}
    }
}