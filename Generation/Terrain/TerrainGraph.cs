using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.GraphCreator;
using System;
using JStuff.Generation;

namespace JStuff.Generation.Terrain
{
    [CreateAssetMenu(menuName = "JStuff/Terrain Graph")]

    public class TerrainGraph : Graph
    {

        public float sizeOfChunk = 10;
        public int seed = 42;

        PropertyPort<Vector2> chunkPosition;
        PropertyPort<Vector2> centerPosition;
        PropertyPort<bool> changed;
        PropertyPort<float> chunkSize;
        PropertyPort<int> seedProperty;
        PropertyPort<float> scale;
        PropertyPort<float> zoom;

        public float Zoom
        {
            set
            {
                zoom.cachedValue = value;
            }
        }

        public float Scale
        {
            set
            {
                scale.cachedValue = value;
            }
        }

        public Vector2 ChunkPosition
        {
            set
            {
                changed.cachedValue = true;
                chunkPosition.cachedValue = value;
            }
        }

        public Vector2 CenterPosition
        {
            set
            {
                changed.cachedValue = true;
                centerPosition.cachedValue = value;
            }
        }

        public float ChunkSize
        {
            set
            {
                chunkSize.cachedValue = value;
            }
        }

        public int Seed
        {
            set
            {
                seedProperty.cachedValue = value;
            }
        }

        public override Type RootNodeType => typeof(TerrainRoot);
        public override List<Type> NodeTypes
        {
            get
            {
                List<Type> types = new List<Type>();
                types.Add(typeof(EquilateralTriangleNode));
                types.Add(typeof(CreateMesh));
                types.Add(typeof(CreateColormap));
                types.Add(typeof(SlopeColormap));
                types.Add(typeof(CommonNode));
                types.Add(typeof(VectorDivNode));
                types.Add(typeof(GrayScale));
                types.Add(typeof(RandomTerrainObjects));
                types.Add(typeof(Uniform2DPoints));
                return types;
            }
        }

        public BlockData EvaluateGraph()
        {
            return ((TerrainRoot)rootNode).Evaluate();
        }

        protected override void SetupProperties()
        {
            chunkPosition = AddProperty(Vector2.zero, "chunkPosition", PropertyContext.Unique);
            centerPosition = AddProperty(Vector2.zero, "centerPosition", PropertyContext.Unique);
            changed = AddProperty(false, "changed", PropertyContext.Unique);
            chunkSize = AddProperty(sizeOfChunk, "chunkSize", PropertyContext.Unique);
            seedProperty = AddProperty(seed, "seed", PropertyContext.Unique);
            AddProperty(Vector2.zero, "offset", PropertyContext.Unique);
            scale = AddProperty<float>(1, "scale", PropertyContext.Unique);
            zoom = AddProperty<float>(100, "zoom", PropertyContext.Unique);
        }
    }
}