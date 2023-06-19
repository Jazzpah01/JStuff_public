using JStuff.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.AI.Pathfinding
{
    /// <summary>
    /// A grid graph of T with weight and existance of edge determined on a function of T.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GridGraph : IGraph<Vector2>
    {
        Vector2 offset;
        Vector2 stride;
        Vector2 minimumWeight;
        NeighborWeight[] weights;
        List<int>[] neighbors;

        int length0;
        int length1;

        public int Length0 => length0;
        public int Length1 => length1;
        public int Length => length0 * length1;
        public Vector2 Stride => stride;

        public struct NeighborWeight
        {
            public float r;
            public float u;
            public float l;
            public float d;
            public float ru;
            public float ul;
            public float ld;
            public float dr;
        }

        public GridGraph(Vector2[,] nodes)
        {
            this.length0 = nodes.GetLength(0);
            this.length1 = nodes.GetLength(1);
            this.offset = nodes[0, 0];
            this.stride = new Vector2(Mathf.Abs(nodes[0, 0].x - nodes[1, 0].x), Mathf.Abs(nodes[0, 0].y - nodes[0, 1].y));
            this.weights = new NeighborWeight[length0 * length1];
            this.neighbors = new List<int>[length0 * length1];
        }

        public GridGraph(Vector2 offset, NeighborWeight[] weights, List<int>[] neighbors, int length0, int length1, Vector2 stride)
        {
            this.offset = offset;
            this.weights = new NeighborWeight[length0 * length1];
            this.neighbors = new List<int>[length0 * length1];
            this.length0 = length0;
            this.length1 = length1;

            if (weights != null)
            {
                for (int i = 0; i < weights.Length; i++)
                {
                    this.weights[i] = weights[i];
                }
            }

            if (neighbors != null)
            {
                for (int i = 0; i < neighbors.Length; i++)
                {
                    if (neighbors[i] == null || neighbors[i].Count == 0)
                        continue;

                    for (int j = 0; j < neighbors[i].Count; j++)
                    {
                        this.neighbors[i].Add(neighbors[i][j]);
                    }
                }
            }

            this.stride = stride;
        }

        public void ConstructEdges<T>(T[,] nodeValues, Func<T, T, bool> hasEdge, Func<T, T, float> weightFunction, EdgeOptions edgeOptions)
        {
            if (nodeValues == null)
                throw new Exception("nodeValues cannot be null.");

            float diagonalStride = stride.magnitude;

            neighbors = new List<int>[length0 * length1];

            for (int y = 0; y < length1; y++)
            {
                for (int x = 0; x < length0; x++) // TODO: Optimisation
                {
                    int index = x + y * length0;

                    this.neighbors[index] = new List<int>();

                    this.weights[index] = new NeighborWeight();

                    float weight = weightFunction(nodeValues[x, y], nodeValues[x, y]); // TODO: Optimisation
                    float horizontalWeight = weight;
                    float verticalWeight = weight;
                    float diagonalWeight = weight;

                    if (edgeOptions.HasFlag(EdgeOptions.MultiplyDistance))
                    {
                        horizontalWeight *= stride.x;
                        verticalWeight   *= stride.y;
                        diagonalWeight   *= diagonalStride;
                    }
                    if (edgeOptions.HasFlag(EdgeOptions.AddDistance))
                    {
                        horizontalWeight += stride.x;
                        verticalWeight += stride.y;
                        diagonalWeight += diagonalStride;
                    }

                    if ((x + 1).InRange(0, length0 - 1) && hasEdge(nodeValues[x, y], nodeValues[x + 1, y]))
                    {
                        AddDirectedEdge(x + length0 * y, (x + 1) + length0 * y, horizontalWeight);
                    }
                    if ((x - 1).InRange(0, length0 - 1) && hasEdge(nodeValues[x, y], nodeValues[x - 1, y]))
                    {
                        AddDirectedEdge(x + length0 * y, (x - 1) + length0 * y, horizontalWeight);
                    }
                    if ((y + 1).InRange(0, length1 - 1) && hasEdge(nodeValues[x, y], nodeValues[x, y + 1]))
                    {
                        AddDirectedEdge(x + length0 * y, x + length0 * (y + 1), verticalWeight);
                    }
                    if ((y - 1).InRange(0, length1 - 1) && hasEdge(nodeValues[x, y], nodeValues[x, y - 1]))
                    {
                        AddDirectedEdge(x + length0 * y, x + length0 * (y - 1), verticalWeight);
                    }

                    if (edgeOptions.HasFlag(EdgeOptions.UseDiagonals))
                    {
                        if ((x + 1).InRange(0, length0 - 1) && hasEdge(nodeValues[x, y], nodeValues[x + 1, y]) &&
                            (y + 1).InRange(0, length1 - 1) && hasEdge(nodeValues[x, y], nodeValues[x, y + 1]))
                        {
                            AddDirectedEdge(x + length0 * y, (x + 1) + length0 * (y + 1), diagonalWeight);
                        }
                        if ((x + 1).InRange(0, length0 - 1) && hasEdge(nodeValues[x, y], nodeValues[x + 1, y]) &&
                            (y - 1).InRange(0, length1 - 1) && hasEdge(nodeValues[x, y], nodeValues[x, y - 1]))
                        {
                            AddDirectedEdge(x + length0 * y, (x + 1) + length0 * (y - 1), diagonalWeight);
                        }
                        if ((x - 1).InRange(0, length0 - 1) && hasEdge(nodeValues[x, y], nodeValues[x - 1, y]) &&
                            (y + 1).InRange(0, length1 - 1) && hasEdge(nodeValues[x, y], nodeValues[x, y + 1]))
                        {
                            AddDirectedEdge(x + length0 * y, (x - 1) + length0 * (y + 1), diagonalWeight);
                        }
                        if ((x - 1).InRange(0, length0 - 1) && hasEdge(nodeValues[x, y], nodeValues[x - 1, y]) &&
                            (y - 1).InRange(0, length1 - 1) && hasEdge(nodeValues[x, y], nodeValues[x, y - 1]))
                        {
                            AddDirectedEdge(x + length0 * y, (x - 1) + length0 * (y - 1), diagonalWeight);
                        }
                    }
                }
            }
        }

        public void AddUndirectedEdge(int v, int u, float weight)
        {
            AddDirectedEdge(v, u, weight);
            AddDirectedEdge(u, v, weight);
        }

        public void AddDirectedEdge(int v, int u, float weight)
        {
            if (weight <= 0)
                throw new System.Exception("Can't have non-positive weights.");

            int diff = u - v;
            if (diff == 1)
            {
                weights[v].r = weight;
            }
            if (diff == -1)
            {
                weights[v].l = weight;
            }
            if (diff == length0)
            {
                weights[v].u = weight;
            }
            if (diff == -length0)
            {
                weights[v].d = weight;
            }

            if (diff == 1 + length0)
            {
                weights[v].ru = weight;
            }
            if (diff == 1 - length0)
            {
                weights[v].dr = weight;
            }
            if (diff == -1 + length0)
            {
                weights[v].ul = weight;
            }
            if (diff == -1 - length0)
            {
                weights[v].ld = weight;
            }


            this.weights[v].r = weight;
            this.neighbors[v].Add(u);
            //TODO: Do this optimisation:
            //if (weight < this.Dx) this.Dx = weight;
        }

        public int Size => Length;

        public IList<int> AdjacentNodes(int index)
        {
            return neighbors[index];
        }

        public bool Contains(int index)
        {
            return index.InRange(0, Length);
        }

        public float GetWeight(int index_from, int index_to)
        {
            int diff = index_to - index_from;
            if (diff == 1)
            {
                return weights[index_from].r;
            }
            if (diff == -1)
            {
                return weights[index_from].l;
            }
            if (diff == length0)
            {
                return weights[index_from].u;
            }
            if (diff == -length0)
            {
                return weights[index_from].d;
            }

            if (diff == 1 + length0)
            {
                return weights[index_from].ru;
            }
            if (diff == 1 - length0)
            {
                return weights[index_from].dr;
            }
            if (diff == -1 + length0)
            {
                return weights[index_from].ul;
            }
            if (diff == -1 - length0)
            {
                return weights[index_from].ld;
            }

            throw new Exception("Edge does not exist in graph.");
        }

        public int GetIndexOfNode(Vector2 p)
        {
            //Change to get closest!
            int x = Mathf.RoundToInt((p.x - offset.x) / stride.x);
            int y = Mathf.RoundToInt((p.y - offset.y) / stride.y);

            if (x < 0 || x >= length0)
                throw new Exception("x is out of bounds!");
            if (y < 0 || y >= length0)
                throw new Exception("y is out of bounds!");

            return x * length0 + y;
        }

        public Vector2 this[int i, int j]
        {
            get { return new Vector2(i * stride.x, j * stride.y) + offset; }
        }

        public Vector2 this[int i]
        {
            get
            {
                int x = i % length0;
                int y = i / length0;
                return this[x, y];
            }
        }

        public int GetClosestNode(Vector2 p)
        {
            int x = Mathf.RoundToInt((p.x - offset.x) / stride.x);
            int y = Mathf.RoundToInt((p.y - offset.y) / stride.y);

            x = Mathf.Clamp(x, 0, length0-1);
            y = Mathf.Clamp(y, 0, length0-1);

            return x + length0 * y;
        }

        public GridGraph Clone()
        {
            return new GridGraph(offset, weights, neighbors, length0, length1, stride);
        }

        /// <summary>
        /// Heuristic of this graph for A* pathfinding. The manhatten distance. Used when there are no diagonal lines.
        /// Useful: http://theory.stanford.edu/~amitp/GameProgramming/Heuristics.html
        /// </summary>
        /// <param name="v">First node vector.</param>
        /// <param name="u">Second node vector.</param>
        /// <returns></returns>
        public float ManhattenDistance(int v, int u)
        {
            float vy = v % length1;
            float vx = (v - vy) / length0;
            float uy = u % length1;
            float ux = (u - uy) / length0;
            return Mathf.Abs(vx - ux) * stride.x + Mathf.Abs(vy - uy) * stride.y;
        }

        /// <summary>
        /// Heuristic of this graph for A* pathfinding. The diagonal distance. Used when there are diagonal lines.
        /// Useful: http://theory.stanford.edu/~amitp/GameProgramming/Heuristics.html
        /// </summary>
        /// <param name="v">First node vector.</param>
        /// <param name="u">Second node vector.</param>
        /// <returns></returns>
        public float DiagonalDistance(int v, int u)
        {
            float vy = v % length1;
            float vx = (v - vy) / length0;
            float uy = u % length1;
            float ux = (u - uy) / length0;
            float dx = Mathf.Abs(vx - ux);
            float dy = Mathf.Abs(vy - uy);
            return stride.x * (dx + dy) + (stride.magnitude - 2 * stride.y) * Mathf.Min(dx, dy);
        }

        /// <summary>
        /// Heuristic of this graph for A* pathfinding. The euclidian distance.
        /// Useful: http://theory.stanford.edu/~amitp/GameProgramming/Heuristics.html
        /// </summary>
        /// <param name="v">First node vector.</param>
        /// <param name="u">Second node vector.</param>
        /// <returns></returns>
        public float EuclideanDistance(int v, int u)
        {
            float vy = v % length1;
            float vx = (v - vy) / length0;
            float uy = u % length1;
            float ux = (u - uy) / length0;
            float dx = Mathf.Abs(vx - ux);
            float dy = Mathf.Abs(vy - uy);
            return Mathf.Sqrt(dx * dx + dy * dy);
        }
    }
}