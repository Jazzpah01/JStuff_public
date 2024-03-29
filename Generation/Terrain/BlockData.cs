using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.Generation.Terrain
{
    public class BlockData
    {
        public MeshData meshRendererData;
        public MeshData meshColliderData;
        public Color[] colormap;
        public List<TerrainObject> terrainObjects;
        public List<(FoliageObject, List<List<Matrix4x4>>)> foliage;

        public MeshData _meshRendererData;
        public Color[] _colormap;
        public Vector3[] _normals;
        public MeshData _meshColliderData;

        public bool[] seamExtrusion;
        public int[] seamSize;
    }
}