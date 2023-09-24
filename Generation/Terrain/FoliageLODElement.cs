using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[System.Serializable]
public class FoliageLODElement
{
    public float distance;
    public int LOD;

    public FoliageLODElement(float distance, int lOD)
    {
        this.distance = distance;
        LOD = lOD;
    }
}