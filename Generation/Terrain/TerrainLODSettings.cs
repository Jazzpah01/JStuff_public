using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "JStuff/Terrain/Terrain LOD Settings")]
public class TerrainLODSettings : ScriptableObject
{
    public enum LODTypes
    {
        Mesh,
        TerrainObjects,
    }

    public float worldTerrainDistance = 500;
    public float worldTerrainUpdateDistance = 100f;

    public float worldTerrainMaxWorkload_Ms = 5f;
    public float terrainPoolWorkloadTarget_Ms = 2;
    public float terrainPoolWorkloadMax_Ms = 4;

    public float jobPenalty_terrainObjects = 0.01f;
    public float jobPenalty_renderMesh = 0.1f;
    public float jobPenalty_collider = 0.1f;
    public float jobPenalty_foliage = 0.1f;

    public AnimationCurve terrainPoolWorkloadCurve;

    [Range(0, 1)]
    public float foliageCutoff = 0f;

    [Tooltip("Default LOD of mesh when out of bounds. '-1' means no generation.")]
    public int defaultMeshLOD = -1;
    [Tooltip("Default LOD of terrain objects when out of bounds. '-1' means no generation.")]
    public int defaultTerrainObjectLOD = -1;

    public List<TerrainLODElement> terrainLOD;

    public int GetLOD(float distance, LODTypes lodType)
    {
        if (terrainLOD == null || terrainLOD.Count == 0)
            throw new System.Exception("");

        for (int i = 0; i < terrainLOD.Count; i++)
        {
            if (terrainLOD[i].distance >= distance)
            {
                switch (lodType)
                {
                    case LODTypes.Mesh:
                        return terrainLOD[i].meshLOD;
                    case LODTypes.TerrainObjects:
                        return terrainLOD[i].terrainObjectLOD;
                }
            }
        }

        switch (lodType)
        {
            case LODTypes.Mesh:
                return defaultMeshLOD;
            case LODTypes.TerrainObjects:
                return defaultTerrainObjectLOD;
            default:
                throw new System.Exception("???");
        }
    }
}