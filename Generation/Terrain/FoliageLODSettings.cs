using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(menuName ="JStuff/Terrain/Foliage LOD Settings")]
public class FoliageLODSettings : ScriptableObject
{
    [Tooltip("Default LOD of foliage when out of bounds. '-1' means no drawing.")]
    public int defaultFoliageLOD = -1;

    public bool cullChunkOnDistance = true;

    
    //public bool cullInstanceOnDistance = true; TODO: Make the system for this!

    public List<FoliageLODElement> foliageLOD;

    public float CullDistance => foliageLOD[foliageLOD.Count - 1].distance;

    public int maxLOD => foliageLOD[foliageLOD.Count - 1].LOD;
    public int minLOD => foliageLOD[0].LOD;

    public float GetCullDistanceOfLOD(int LOD)
    {
        for (int i = 0; i < foliageLOD.Count; i++)
        {
            if (LOD < foliageLOD[i].LOD)
                return foliageLOD[i].distance;
        }

        return CullDistance;
    }

    public int GetLOD(float distance)
    {
        for (int i = 0; i < foliageLOD.Count; i++)
        {
            if (distance <= foliageLOD[i].distance)
                return foliageLOD[i].LOD;
        }

        return defaultFoliageLOD;
    }
}