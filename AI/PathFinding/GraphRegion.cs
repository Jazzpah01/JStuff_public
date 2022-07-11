using JStuff.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.AI.Pathfinding;

[RequireComponent(typeof(SpriteRenderer))]
public class GraphRegion : Region
{
    public bool stretch = false;
    public float density = 1;

    private GridGraph<Vector2> graph;

    private void Start()
    {
        graph = new GridGraph<Vector2>();

        int xAmount = (int)(Range.x * density)+1;
        int yAmount = (int)(Range.y * density)+1;

        float xCellSize = density / Range.x;
        float yCellSize = density / Range.y;

        List<Vector2> nodes = new List<Vector2>();

        for (int i = 0; i < xAmount; i++)
        {
            for (int j = 0; i < yAmount; i++)
            {
                nodes.Add(new Vector2(this.PositionMin.x + i * xCellSize, this.PositionMin.y + j * yCellSize));
            }
        }

        graph.Construct(nodes.ToArray(), (a, b) => (a - b).magnitude, (a, b) => true, true);
    }
}