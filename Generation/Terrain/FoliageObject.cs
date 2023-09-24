using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(menuName ="Assets/JStuff/Generation/Terrain/Foliage Object")]
public class FoliageObject: ScriptableObject
{
    public List<FoliageLODObject> LODMeshes;

    public FoliageLODObject GetFoliage(int LOD)
    {
        FoliageLODObject retval = LODMeshes[0];

        for (int i = LODMeshes.Count - 1; i > 0; i++)
        {
            if (LOD > LODMeshes[i].LOD)
                retval = LODMeshes[i - 1];
        }

        return retval;
    }

    public int maxLOD => LODMeshes[LODMeshes.Count - 1].LOD;
    public int minLOD => LODMeshes[0].LOD;
}