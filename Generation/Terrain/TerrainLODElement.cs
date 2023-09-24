using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[System.Serializable]
public class TerrainLODElement
{
    public float distance;
    [Tooltip("LOD of the mesh of a chunk, given the distance.")]
    public int meshLOD;
    [Tooltip("LOD defining what terrain objects should spawn, given the distance.")]
    public int terrainObjectLOD;
}