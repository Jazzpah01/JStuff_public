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

        public MeshData _meshRendererData;
        public Color[] _colormap;
        public MeshData _meshColliderData;

        public List<Seam> _seamNormals;
    }
}