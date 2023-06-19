using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using JStuff.Collections;

[CreateAssetMenu(menuName ="JStuff/Generation/Dungeon Template")]
public class CellDungeonTemplate : ScriptableObject
{
    public MaxConnections maxConnections;

    public List<WeightedPrefab> none;
    public List<WeightedPrefab> right;
    public List<WeightedPrefab> up;
    public List<WeightedPrefab> left;
    public List<WeightedPrefab> down;
    public List<WeightedPrefab> right_up;
    public List<WeightedPrefab> up_left;
    public List<WeightedPrefab> left_down;
    public List<WeightedPrefab> right_down;
    public List<WeightedPrefab> right_left;
    public List<WeightedPrefab> up_down;
    public List<WeightedPrefab> right_up_left;
    public List<WeightedPrefab> up_left_down;
    public List<WeightedPrefab> right_left_down;
    public List<WeightedPrefab> right_up_down;
    public List<WeightedPrefab> right_up_left_down;

    public GameObject GetPrefab(Connections connections)
    {
        return GetPrefab(connections, -1);
    }

    public GameObject GetPrefab(Connections connections, int seed)
    {
        WeightedPrefab retval = new WeightedPrefab();

        // Zero connections
        if (connections == Connections.none)
        {
            retval = GetRandomTemplate(none, seed);
        }

        // One connection
        if (connections == Connections.right)
        {
            retval = GetRandomTemplate(right, seed);
        }
        if (connections == Connections.up)
        {
            retval = GetRandomTemplate(up, seed);
        }
        if (connections == Connections.left)
        {
            retval = GetRandomTemplate(left, seed);
        }
        if (connections == Connections.down)
        {
            retval = GetRandomTemplate(down, seed);
        }

        // Four connections
        if (connections.HasFlag(Connections.right | Connections.right | Connections.up | Connections.down))
        {
            retval = GetRandomTemplate(right_up_left_down, seed);
        }

        // Three connections
        if (connections.HasFlag(Connections.right | Connections.up | Connections.left))
        {
            retval = GetRandomTemplate(right_up_left, seed);
        }
        if (connections.HasFlag(Connections.up | Connections.left | Connections.down))
        {
            retval = GetRandomTemplate(up_left_down, seed);
        }
        if (connections.HasFlag(Connections.right | Connections.left | Connections.down))
        {
            retval = GetRandomTemplate(right_left_down, seed);
        }
        if (connections.HasFlag(Connections.right | Connections.up | Connections.down))
        {
            retval = GetRandomTemplate(right_up_down, seed);
        }

        // Two connections
        if (connections.HasFlag(Connections.right | Connections.up))
        {
            retval = GetRandomTemplate(right_up, seed);
        }
        if (connections.HasFlag(Connections.up | Connections.left))
        {
            retval = GetRandomTemplate(up_left, seed);
        }
        if (connections.HasFlag(Connections.left | Connections.down))
        {
            retval = GetRandomTemplate(left_down, seed);
        }
        if (connections.HasFlag(Connections.right | Connections.down))
        {
            retval = GetRandomTemplate(right_down, seed);
        }

        if (connections.HasFlag(Connections.up | Connections.down))
        {
            retval = GetRandomTemplate(up_down, seed);
        }
        if (connections.HasFlag(Connections.right | Connections.left))
        {
            retval = GetRandomTemplate(right_left, seed);
        }

        return retval.prefab;
    }

    private WeightedPrefab GetRandomTemplate(List<WeightedPrefab> list, int seed = -1)
    {
        float totalWeight = list.Aggregate(0f, (acc, elm) => acc + elm.weight);

        System.Random randomGenerator = null;

        if (seed >= 0)
        {
            randomGenerator = new System.Random(seed);
        } else
        {
            randomGenerator = new System.Random();
        }

        float r = Mathf.Clamp((float)randomGenerator.NextDouble() * totalWeight, 0f, totalWeight);
        float acc = 0;
        WeightedPrefab retval = new WeightedPrefab();

        for (int i = 0; i < list.Count; i++)
        {
            if (r < acc + list[i].weight)
            {
                acc += list[i].weight;
                retval = list[i];
            } else
            {
                break;
            }
        }

        return retval;
    }
}
