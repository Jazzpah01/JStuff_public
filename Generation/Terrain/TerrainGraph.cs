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

        public float chunkSize = 10;
        public int seed = 42;
        public float zoom = 50;
        public float scale = 1;

        PropertyPort<Vector2> chunkPositionProperty;
        PropertyPort<Vector2> centerPositionProperty;
        PropertyPort<float> chunkSizeProperty;
        PropertyPort<int> seedProperty;
        PropertyPort<float> scaleProperty;
        PropertyPort<float> zoomProperty;

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

        public override Type RootNodeType => typeof(TerrainRoot);
        public override List<Type> NodeTypes
        {
            get
            {
                List<Type> types = new List<Type>();
                types.Add(typeof(CommonNode));
                types.Add(typeof(MathNode));
                types.Add(typeof(Generation));
                types.Add(typeof(TerrainUtility));
                return types;
            }
        }

        public BlockData EvaluateGraph()
        {
            return ((TerrainRoot)rootNode).Evaluate();
        }

        protected override void SetupProperties()
        {
            chunkPositionProperty = AddProperty(Vector2.zero, "chunkPosition", PropertyContext.Unique);
            centerPositionProperty = AddProperty(Vector2.zero, "centerPosition", PropertyContext.Unique);
            chunkSizeProperty = AddProperty(chunkSize, "chunkSize", PropertyContext.Unique);
            seedProperty = AddProperty(seed, "seed", PropertyContext.Unique);
            AddProperty(Vector2.zero, "offset", PropertyContext.Unique);
            scaleProperty = AddProperty<float>(scale, "scale", PropertyContext.Unique);
            zoomProperty = AddProperty<float>(zoom, "zoom", PropertyContext.Unique);
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