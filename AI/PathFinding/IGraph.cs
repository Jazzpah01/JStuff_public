using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.AI.Pathfinding
{
    public interface IGraph<T>
    {
        IList<int> AdjacentNodes(int index);
        float GetWeight(int index_from, int index_to);
        bool Contains(int index);
        int GetIndexOfNode(T node);
        int Size { get; }
        T this[int i] { get; }
    }
}