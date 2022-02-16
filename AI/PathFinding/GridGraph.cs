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
        Vector2[] nodes;
        NeighborWeight[] weight;
        List<int>[] neighbors;
        int size;

        int length0;
        int length1;

        float D;
        float D2; // TODO: find this

        Vector2 offset;
        float cellDistance;

        bool initialized = false;

        bool diagonalEdges;

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

        public void Construct<T>(T[,] nodes, Func<T, T, float> weightFunction, Func<T, T, bool> hasEdge, bool diagonalEdges, Vector2 offset, float xSize = 1)
        {
            this.initialized = true;

            this.nodes = new Vector2[nodes.GetLength(0) * nodes.GetLength(1)];
            this.weight = new NeighborWeight[nodes.GetLength(0) * nodes.GetLength(1)];
            this.neighbors = new List<int>[nodes.GetLength(0) * nodes.GetLength(1)];
            this.size = nodes.GetLength(0) * nodes.GetLength(1);

            float xDistance = xSize / (nodes.GetLength(0) - 1);

            this.D = float.MaxValue;
            this.D2 = float.MaxValue;

            this.offset = offset;
            this.cellDistance = xDistance;

            this.length0 = nodes.GetLength(0);
            this.length1 = nodes.GetLength(1);

            this.diagonalEdges = diagonalEdges;

            for (int i = 0; i < nodes.GetLength(0); i++)
            {
                for (int j = 0; j < nodes.GetLength(1); j++)
                {
                    Vector2 newNode = new Vector2(i * xDistance + offset.x, j * xDistance + offset.y);

                    int index = i * nodes.GetLength(0) + j;
                    //int index = i * nodes.GetLength(1) + j;
                    //int index = i + j * nodes.GetLength(0);

                    this.neighbors[index] = new List<int>();

                    this.weight[index] = new NeighborWeight();
                    this.nodes[index] = newNode;

                    if ((i + 1).InRange(0, nodes.GetLength(0) - 1) && hasEdge(nodes[i, j], nodes[i + 1, j]))
                    {
                        float nweight = weightFunction(nodes[i, j], nodes[i + 1, j]);
                        if (nweight <= 0) throw new System.Exception("Can't have non-positive weight.");
                        this.weight[index].r = nweight;
                        this.neighbors[index].Add(index + nodes.GetLength(0));
                        if (nweight < this.D) this.D = nweight;
                    }
                    if ((i - 1).InRange(0, nodes.GetLength(0) - 1) && hasEdge(nodes[i, j], nodes[i - 1, j]))
                    {
                        float nweight = weightFunction(nodes[i, j], nodes[i - 1, j]);
                        if (nweight <= 0) throw new System.Exception("Can't have non-positive weight.");
                        this.weight[index].l = nweight;
                        this.neighbors[index].Add(index - nodes.GetLength(0));
                        if (nweight < this.D) this.D = nweight;
                    }
                    if ((j + 1).InRange(0, nodes.GetLength(1) - 1) && hasEdge(nodes[i, j], nodes[i, j + 1]))
                    {
                        float nweight = weightFunction(nodes[i, j], nodes[i, j + 1]);
                        if (nweight <= 0) throw new System.Exception("Can't have non-positive weight.");
                        this.weight[index].u = nweight;
                        this.neighbors[index].Add(index + 1);
                        if (nweight < this.D) this.D = nweight;
                    }
                    if ((j - 1).InRange(0, nodes.GetLength(1) - 1) && hasEdge(nodes[i, j], nodes[i, j - 1]))
                    {
                        float nweight = weightFunction(nodes[i, j], nodes[i, j - 1]);
                        if (nweight <= 0) throw new System.Exception("Can't have non-positive weight.");
                        this.weight[index].d = nweight;
                        this.neighbors[index].Add(index - 1);
                        if (nweight < this.D) this.D = nweight;
                    }

                    if (diagonalEdges)
                    {
                        if ((i + 1).InRange(0, nodes.GetLength(0) - 1) && hasEdge(nodes[i, j], nodes[i + 1, j]) &&
                            (j + 1).InRange(0, nodes.GetLength(1) - 1) && hasEdge(nodes[i, j], nodes[i, j + 1]))
                        {
                            float nweight = weightFunction(nodes[i, j], nodes[i + 1, j + 1]);
                            if (nweight <= 0) throw new System.Exception("Can't have non-positive weight.");
                            this.weight[index].ru = nweight;
                            this.neighbors[index].Add(index + nodes.GetLength(0) + 1);
                            if (nweight < this.D2) this.D2 = nweight;
                        }
                        if ((i - 1).InRange(0, nodes.GetLength(0) - 1) && hasEdge(nodes[i, j], nodes[i - 1, j]) &&
                            (j + 1).InRange(0, nodes.GetLength(1) - 1) && hasEdge(nodes[i, j], nodes[i, j + 1]))
                        {
                            float nweight = weightFunction(nodes[i, j], nodes[i - 1, j + 1]);
                            if (nweight <= 0) throw new System.Exception("Can't have non-positive weight.");
                            this.weight[index].ul = nweight;
                            this.neighbors[index].Add(index - nodes.GetLength(0) + 1);
                            if (nweight < this.D2) this.D2 = nweight;
                        }
                        if ((i + 1).InRange(0, nodes.GetLength(0) - 1) && hasEdge(nodes[i, j], nodes[i + 1, j]) &&
                            (j - 1).InRange(0, nodes.GetLength(1) - 1) && hasEdge(nodes[i, j], nodes[i, j - 1]))
                        {
                            float nweight = weightFunction(nodes[i, j], nodes[i + 1, j - 1]);
                            if (nweight <= 0) throw new System.Exception("Can't have non-positive weight.");
                            this.weight[index].dr = nweight;
                            this.neighbors[index].Add(index + nodes.GetLength(0) - 1);
                            if (nweight < this.D2) this.D2 = nweight;
                        }
                        if ((i - 1).InRange(0, nodes.GetLength(0) - 1) && hasEdge(nodes[i, j], nodes[i - 1, j]) &&
                            (j - 1).InRange(0, nodes.GetLength(1) - 1) && hasEdge(nodes[i, j], nodes[i, j - 1]))
                        {
                            float nweight = weightFunction(nodes[i, j], nodes[i - 1, j - 1]);
                            if (nweight <= 0) throw new System.Exception("Can't have non-positive weight.");
                            this.weight[index].ld = nweight;
                            this.neighbors[index].Add(index - nodes.GetLength(0) - 1);
                            if (nweight < this.D2) this.D2 = nweight;
                        }
                    }
                }
            }
        }

        public Vector2 this[int i]
        {
            get
            {
                return nodes[i];
            }
        }

        public int Size => size;

        public int[] AdjacentNodes(int index)
        {
            return neighbors[index].ToArray();
        }

        public bool Contains(int index)
        {
            return index.InRange(0, size);
        }

        public float GetWeight(int index_from, int index_to)
        {
            int diff = index_to - index_from;
            if (diff == 1)
            {
                return weight[index_from].u;
            }
            if (diff == -1)
            {
                return weight[index_from].d;
            }
            if (diff == length1)
            {
                return weight[index_from].l;
            }
            if (diff == -length1)
            {
                return weight[index_from].r;
            }
            if (!diagonalEdges)
                throw new Exception("Weight does not exist in graph.");
            if (diff == 1 + length1)
            {
                return weight[index_from].ru;
            }
            if (diff == 1 - length1)
            {
                return weight[index_from].dr;
            }
            if (diff == -1 + length1)
            {
                return weight[index_from].ul;
            }
            if (diff == -1 - length1)
            {
                return weight[index_from].ld;
            }

            throw new Exception("Weight does not exist in graph.");
        }

        public int GetIndexOfNode(Vector2 node)
        {
            int x = Mathf.RoundToInt((node.x - offset.x) / cellDistance);
            int y = Mathf.RoundToInt((node.y - offset.y) / cellDistance);

            return x * length0 + y;

            //return (int)node.x * nodeGrid.GetLength(0) + (int)node.y; <- how does that even work?
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
            return (Mathf.Abs(vx - ux) + Mathf.Abs(vy - uy)) * D;
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
            return D * (dx + dy) + (D2 - 2 * D) * Mathf.Min(dx, dy);
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