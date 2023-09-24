using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[System.Serializable]
public class FoliageLODObject
{
    public int LOD;
    public Mesh mesh;
    public List<Material> materials;
}