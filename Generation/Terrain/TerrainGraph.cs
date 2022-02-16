using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.GraphCreator;
using System;

[CreateAssetMenu(menuName ="Assets/JStuff/Terrain Graph")]
public class TerrainGraph : Graph
{

    public float sizeOfChunk = 10;

    PropertyPort<Vector2> chunkPosition;
    PropertyPort<Vector2> centerPosition;
    PropertyPort<bool> changed;
    PropertyPort<float> chunkSize;

    public Vector2 ChunkPosition {
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

    public override Type RootNodeType => typeof(TerrainRoot);
    public override List<Type> NodeTypes
    {
        get
        {
            List<Type> types = new List<Type>();
            types.Add(typeof(CreateHeightMap));
            types.Add(typeof(CreateMesh));
            types.Add(typeof(CreateColormap));
            types.Add(typeof(CommonNode));
            return types;
        }
    }

    public BlockData EvaluateGraph()
    {
        return ((TerrainRoot)rootNode).Evaluate();
    }

    protected override void SetupProperties()
    {
        chunkPosition = AddProperty<Vector2>(Vector2.zero, "chunkPosition", PropertyContext.Unique);
        centerPosition = AddProperty<Vector2>(Vector2.zero, "centerPosition", PropertyContext.Shared);
        changed = AddProperty<bool>(false, "changed", PropertyContext.Unique);
        AddProperty<float>(sizeOfChunk, "chunkSize", PropertyContext.Shared);
    }
}