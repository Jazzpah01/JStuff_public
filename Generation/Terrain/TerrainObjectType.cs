using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public struct TerrainObjectType: IWeightedPrefab
{
    public GameObject prefab;
    public float objectRadius;
    public float maxSlope;
    public float minHeight;
    public float maxHeight;
    [Range(0f, 1f)]
    public float weight;
    public float scaleVar;
    public float scaleChange;

    public GameObject Prefab => prefab;

    public float Weight => weight;
}