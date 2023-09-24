using JStuff.GraphCreator;
using System;
using UnityEngine;

namespace JStuff.Generation.Terrain
{
    [CreateAssetMenu(menuName = "JStuff/Terrain Graph")]

    public class TerrainGraph : Graph
    {

        public float chunkSize = 10;
        public int seed = 42;
        public float zoom = 50;
        public float scale = 1;

        PropertyLink<Vector2> chunkPositionProperty;
        PropertyLink<Vector2> centerPositionProperty;
        PropertyLink<float> chunkSizeProperty;
        PropertyLink<int> seedProperty;
        PropertyLink<float> scaleProperty;
        PropertyLink<float> zoomProperty;
        PropertyLink<int> LODProperty;
        PropertyLink<int> meshLODProperty;
        PropertyLink<int> terrainObjectLODProperty;
        PropertyLink<int> foliageLODProperty;

        public float Zoom
        {
            set
            {
                if (initialized)
                {
                    zoomProperty.cachedValue = value;
                } else
                {
                    zoom = value;
                }
            }
            get
            {
                if (initialized)
                {
                    return zoomProperty.cachedValue;
                } else
                {
                    return zoom;
                }
            }
        }

        public float Scale
        {
            set
            {
                if (initialized)
                {
                    scaleProperty.cachedValue = value;
                }
                else
                {
                    scale = value;
                }
            }
            get
            {
                if (initialized)
                {
                    return scaleProperty.cachedValue;
                }
                else
                {
                    return scale;
                }
            }
        }

        public Vector2 ChunkPosition
        {
            set
            {
                if (!initialized)
                    throw new System.Exception("Cannot set chunk position of uninitialized graph!");
                chunkPositionProperty.cachedValue = value;
            }
            get
            {
                if (!initialized)
                    throw new System.Exception("Cannot get chunk position of uninitialized graph!");
                return chunkPositionProperty.cachedValue;
            }
        }

        public Vector2 CenterPosition
        {
            set
            {
                if (!initialized)
                    throw new System.Exception("Cannot set center position of uninitialized graph!");
                centerPositionProperty.cachedValue = value;
            }
            get
            {
                if (!initialized)
                    throw new System.Exception("Cannot get center position of uninitialized graph!");
                return centerPositionProperty.cachedValue;
            }
        }

        public void SetChunk(Vector2 chunkPosition, Vector2 centerPosition, int meshLOD, int terrainObjectLOD)
        {
            if (!initialized)
                throw new System.Exception("Cannot set chunk position of uninitialized graph!");

            chunkPositionProperty.cachedValue = chunkPosition;
            centerPositionProperty.cachedValue = centerPosition;
            meshLODProperty.cachedValue = meshLOD;
            terrainObjectLODProperty.cachedValue = terrainObjectLOD;
        }

        public float ChunkSize
        {
            set
            {
                if (initialized)
                {
                    chunkSizeProperty.cachedValue = value;
                }
                else
                {
                    chunkSize = value;
                }
            }
            get
            {
                if (initialized)
                {
                    return chunkSizeProperty.cachedValue;
                }
                else
                {
                    return chunkSize;
                }
            }
        }

        public int Seed
        {
            set
            {
                if (initialized)
                {
                    seedProperty.cachedValue = value;
                }
                else
                {
                    seed = value;
                }
            }
            get
            {
                if (initialized)
                {
                    return seedProperty.cachedValue;
                }
                else
                {
                    return seed;
                }
            }
        }

        public int LOD
        {
            set
            {
                if (initialized)
                {
                    LODProperty.cachedValue = value;
                }
            }
        }

        public override Type RootNodeType => typeof(TerrainRoot);
        public override Type[] NodeTypes
        {
            get
            {
                return new Type[] { 
                    typeof(CommonNode), 
                    typeof(MathNode), 
                    typeof(TerrainNode) };
            }
        }

        public override bool Collapsable => true;

        public BlockData EvaluateGraph(TerrainRoot.BlockDataType toEvaluate = TerrainRoot.BlockDataType.All)
        {
            return ((TerrainRoot)rootNode).Evaluate(toEvaluate);
        }

        protected override void SetupProperties()
        {
            chunkPositionProperty = AddProperty(Vector2.zero, "chunkPosition", PropertyContext.Unique, false);
            centerPositionProperty = AddProperty(Vector2.zero, "centerPosition", PropertyContext.Unique, false);
            chunkSizeProperty = AddProperty(chunkSize, "chunkSize", PropertyContext.Shared, true);
            seedProperty = AddProperty(seed, "seed", PropertyContext.Shared, true);
            AddProperty(Vector2.zero, "offset", PropertyContext.Shared, false);
            scaleProperty = AddProperty<float>(scale, "scale", PropertyContext.Shared, true);
            zoomProperty = AddProperty<float>(zoom, "zoom", PropertyContext.Shared, true);
            LODProperty = AddProperty<int>(1, "lod", PropertyContext.Unique, false);

            meshLODProperty = AddProperty<int>(1, "meshLOD", PropertyContext.Unique);
            terrainObjectLODProperty = AddProperty<int>(1, "terrainObjectLOD", PropertyContext.Unique);
            foliageLODProperty = AddProperty(1, "foliageLOD", PropertyContext.Unique);
        }

        public override Graph Clone()
        {
            TerrainGraph retval = base.Clone() as TerrainGraph;

            retval.ChunkSize = ChunkSize;
            retval.Scale = Scale;
            retval.Zoom = Zoom;
            retval.Seed = Seed;

            return retval;
        }
    }
}