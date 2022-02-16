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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="G">Graph.</param>
        /// <param name="s">Starting vertex.</param>
        /// <param name="t">Endind vertex/goal.</param>
        /// <returns>Returns array of shortest path from s to t INVERTED (t, ... , s).</returns>
        //public static Node[] AStar(Graph G, Node s, Node t)
        //{
        //    // Distance dist(v) to u is g(s->v) + h(v->t)
        //    // where g is the current shortest distance and h is a heuristic function mapping
        //    // an approximate distance between some vertex u and t. s is the starting vertex, 
        //    // u is some vertex, t is the goal vertex.
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
        //    rdist[s] = ManhattenDistance(s, t, 1);
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
        //                if (!S.Contains(u))
        //                    S.Add(u);
        //                dist[u] = dist[v] + G.GetWeight(v, u);
        //                rdist[u] = dist[u] + ManhattenDistance(u, t, 1);
        //                prev[u] = v;
        //            }
        //        }
        //    }
        //
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


        public static N[] AStar<N>(IGraph<N> G, int s, int t, Heuristic heuristic, bool invertedRoute = false, HashSet<int> excludedVertices = null)
        {
            // Distance f(v) to u is g(v) + h(v). g is shortest path s->v, h is heuristic for v->t.
            // where g is the current shortest distance and h is a heuristic function mapping
            // an approximate distance between some vertex u and t. s is the starting vertex, 
            // u is some vertex, t is the goal vertex.

            // Sanitize input
            if (s == t)
            {
                throw new System.Exception("Can't find path between two identical vertices.");
            }

            if (!G.Contains(s))
                throw new System.Exception("Can't find path when initial vertex s is not contained in the graph. Index of s is: " + s);

            if (!G.Contains(s))
                throw new System.Exception("Can't find path when destination vertex t is not contained in the graph. Index of t is: " + t);

            // Begin algorithm
            float[] g = new float[G.Size]; // Current shortest distance to node
            float[] f = new float[G.Size]; // Current shortest distance + estimated distance to goal
            int[] prev = new int[G.Size]; // Previous node of index
            prev.Populate<int>(-1);
            g.Populate<float>(Mathf.Infinity);

            List<int> S = new List<int>();
            List<int> T = new List<int>();

            S.Add(s);
            f[s] = heuristic(s, t);
            g[s] = 0;

            bool goalReached = false;

            int control = 1000;
            while (S.Count > 0 || goalReached)
            {
                int v = S[0];
                int index = 0;

                for (int i = 1; i < S.Count; i++)
                {
                    if (f[S[i]] < f[v])
                    {
                        v = S[i];
                        index = i;
                    }
                }

                S.Remove(v);
                T.Add(v);

                if (v == t)
                {
                    goalReached = true;
                    break;
                }

                foreach (int u in G.AdjacentNodes(v))
                {
                    if (T.Contains(u))
                        continue;

                    float tentative_cost = g[v] + G.GetWeight(v, u);
                    // TODO: Need to check for excluded vertices
                    if (((g[u] == 0) || (tentative_cost < g[u])) && u != s)
                    {
                        if (!S.Contains(u))
                            S.Add(u);
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

                    route.Add(G[p]);
                    p = prev[p];
                }
                route.Add(G[s]);

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