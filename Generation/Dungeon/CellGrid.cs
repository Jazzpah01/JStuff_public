using JStuff.AI.Pathfinding;
using System.Collections;
using System.Collections.Generic;

public class CellGrid: IGraph<(int x,int y)>
{
    List<int>[] neighbors;
    Dictionary<int, float>[] weights;

    public int Length0 { get; private set; }
    public int Length1 { get; private set; }
    public int TotalLength { get; private set; }

    public CellGrid(int x, int y)
    {
        this.Length0 = x;
        this.Length1 = y;
        this.TotalLength = x * y;

        this.neighbors = new List<int>[Length0 * Length1];
        this.weights = new Dictionary<int, float>[Length0 * Length1];
    }

    public void AddEdge(int v, int u)
    {
        AddDirectedEdge(v, u, 1f);
        AddDirectedEdge(u, v, 1f);
    }

    public void AddEdge(int v, int u, float weight)
    {
        AddDirectedEdge(v, u, weight);
        AddDirectedEdge(u, v, weight);
    }

    public void RemoveEdge(int v, int u)
    {
        if (neighbors[v].Contains(u))
        {
            neighbors[v].Remove(u);
            neighbors[u].Remove(v);
        }
    }

    private void AddDirectedEdge(int v, int u, float weight)
    {
        if (neighbors[v] == null)
            neighbors[v] = new List<int>();

        if (weights[v] == null)
            weights[v] = new Dictionary<int, float>();

        if (!neighbors[v].Contains(u))
            neighbors[v].Add(u);

        if (!weights[v].ContainsKey(u))
            weights[v].Add(u, weight);

        weights[v][u] = weight;
    }

    public (int x, int y) this[int i] => (i % Length0, i / Length0);

    public int this[int x, int y] => x + y * Length0;

    public int Size => Length0 * Length1;

    public object Current => throw new System.NotImplementedException();

    public IList<int> AdjacentNodes(int v)
    {
        return neighbors[v];
    }

    public bool Contains(int index)
    {
        return (index >= 0 && index < Size);
    }

    public int GetIndexOfNode((int x, int y) node)
    {
        return node.x + node.y * Length0;
    }

    /// <summary>
    /// Get weight between two nodes.
    /// </summary>
    /// <param name="v">Index of the first node.</param>
    /// <param name="u">Index of the second node.</param>
    /// <returns></returns>
    public float GetWeight(int v, int u)
    {
        return weights[v][u];
    }

    /// <summary>
    /// Calculate connections used in cell dungeon generation.
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    public Connections GetConnections(int v)
    {
        (int x, int y) = this[v];

        IList<int> adjecentNodes = AdjacentNodes(v);

        if (adjecentNodes == null)
            return Connections.none;

        Connections retval = Connections.none;

        if (adjecentNodes.Contains(this[x + 1, y]))
        {
            retval = retval | Connections.right;
        }
        if (adjecentNodes.Contains(this[x - 1, y]))
        {
            retval = retval | Connections.left;
        }
        if (adjecentNodes.Contains(this[x, y + 1]))
        {
            retval = retval | Connections.up;
        }
        if (adjecentNodes.Contains(this[x, y - 1]))
        {
            retval = retval | Connections.down;
        }

        return retval;
    }

    /// <summary>
    /// Creates a copy that contains no edges.
    /// </summary>
    /// <returns>Copy</returns>
    public CellGrid CopyAndRemoveEdges()
    {
        return new CellGrid(Length0, Length1);
    }

    /// <summary>
    /// Creates a copy and creates undirected edges to its grid neighbor.
    /// </summary>
    /// <returns>Copy</returns>
    public CellGrid CopyAndAddEdges()
    {
        CellGrid retval = new CellGrid(Length0, Length1);

        retval.ConnectAllNodes();

        return retval;
    }

    /// <summary>
    /// Connect all nodes to its beighbor with an edge.
    /// </summary>
    public void ConnectAllNodes()
    {
        for (int y = 0; y < Length1; y++)
        {
            for (int x = 0; x < Length0; x++)
            {
                if (x > 0)
                    AddEdge(this[x, y], this[x - 1, y]);

                if (x < Length0 - 1)
                    AddEdge(this[x, y], this[x + 1, y]);

                if (y > 0)
                    AddEdge(this[x, y], this[x, y - 1]);

                if (y < Length0 - 1)
                    AddEdge(this[x, y], this[x, y + 1]);
            }
        }
    }

    public void DisconnectAllNodes()
    {
        for (int i = 0; i < TotalLength; i++)
        {
            neighbors[i] = new List<int>();
        }
    }
}
