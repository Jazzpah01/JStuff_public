using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IWeightedPrefab
{
    GameObject Prefab { get; }
    float Weight { get; }
}
