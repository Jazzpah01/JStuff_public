using JStuff.Generation.Terrain;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public struct TerrainObjectType: IWeightedPrefab, IObjectType
{
    public GameObject prefab;
    
    [Range(0.001f, 1f)]
    public float weight;
    [Min(0)]
    public float scaleVar;
    [Min(0)]
    public float scaleChange;

    public bool rotateToTerrainNormal;

    public GameObject Prefab => prefab;

    public float Weight => weight;
    public float ScaleVar => scaleVar;
    public float ScaleChange => scaleChange;
    public bool RotateToTerrainNormal => rotateToTerrainNormal;
}