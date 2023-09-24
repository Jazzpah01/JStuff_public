using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JStuff.Generation.Terrain
{
    public interface IObjectType
    {
        public float Weight { get; }
        public float ScaleVar { get; }
        public float ScaleChange { get; }

        public bool RotateToTerrainNormal { get; }
    }
}
