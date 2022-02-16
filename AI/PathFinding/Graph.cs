using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace JStuff.AI.Pathfinding
{
    public struct Node
    {
        public float x;
        public float z;

        public override bool Equals(object obj)
        {
            if (obj is Node)
            {
                Node other = (Node)obj;
                return (other.x == x && other.z == z);
            } else
            {
                return base.Equals(obj);
            }
        }
    }





    /// <summary>
    /// A directed graph data-structure.
    /// </summary>
    public class Graph
    {
        private List<Node> nodes;
        private Dictionary<Node, List<Node>> edges;
        private Dictionary<(Node, Node), float> weights;

        public int Size => nodes.Count;
        public Node this[int i] => nodes[i];

        public Graph()
        {
            nodes = new List<Node>();
            edges = new Dictionary<Node, List<Node>>();
            weights = new Dictionary<(Node, Node), float>();
        }

        public Node[] GetNeighbors(Node v)
        {
            if (!edges.ContainsKey(v))
            {
                Debug.LogError("Vertex v not in edges, v = " + v.x + ";" + v.z);
                foreach (Node u in nodes)
                {
                    Debug.Log(u.x + ";" + u.z);
                }
            }

            //return edges[v].Except(disabledVertices).ToArray();
            return edges[v].ToArray();
        }

        public Node[] GetNeighbors(Node v, Node u)
        {
            bool contains = false;
            if (edges[u].Contains(v))
            {
                contains = true;
                edges[u].Remove(v);
            }

            Node[] retval = edges[u].ToArray();

            if (contains)
                edges[u].Add(v);

            //return retval.Except(disabledVertices).ToArray();
            return retval.ToArray();
        }

        public Node[] GetVertices()
        {
            return nodes.ToArray();
        }

        public void AddVertex(Node v)
        {
            if (nodes.Contains(v))
                return;
            nodes.Add(v);
            edges.Add(v, new List<Node>());
        }

        public void AddVertex(float nx, float nz)
        {
            Node v = new Node() { x = nx, z = nz };
            if (nodes.Contains(v))
                return;
            nodes.Add(v);
            edges.Add(v, new List<Node>());
        }

        public void AddEdge(Node v, Node u, float w)
        {
            if (edges[v].Contains(u))
                return;

            if (!nodes.Contains(v))
            {
                Debug.LogError("Vertex v cannot have a neighbour, since v is not contained in the graph. v = " + v.x + ";" + v.z);
                return;
            }
            if (!nodes.Contains(u))
            {
                Debug.LogError("Vertex u cannot be a neighbour, since u is not contained in the graph. u = " + u.x + ";" + u.z);
                return;
            }


            if (w <= 0)
            {
                throw new System.Exception("WARNING: Edge added with non-positive wieght.");
            }

            edges[v].Add(u);
            weights.Add((v, u), w);
        }

        public float GetWeight(Node v, Node u)
        {
            return weights[(v, u)];
        }

        public int[] AdjacentNodes(int index)
        {
            throw new System.NotImplementedException();
        }

        public float GetWeight(int index_from, int index_to)
        {
            throw new System.NotImplementedException();
        }

        public bool Contains(int index)
        {
            throw new System.NotImplementedException();
        }

        public int IndexOfNode(Node node)
        {
            throw new System.NotImplementedException();
        }
    }
}