using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.GraphCreator;
using JStuff.Generation;

public class CreateHeightMap : Node
{
    public int size = 65;
    public int depth = 9;
    public int seed = 42;
    public float zoom = 100;

    public float h = 0.35f;
    public float d = 0.35f;

    HeightMap currentHeightMap;

    OutputLink<HeightMap> output;

    protected override void SetupPorts()
    {
        output = AddOutputLink<HeightMap>(GenerateHeightMap, UnityEditor.Experimental.GraphView.Port.Capacity.Multi);
    }

    public HeightMap GenerateHeightMap()
    {
        EquilateralTriangle equilateralTriangle = new EquilateralTriangle();
        currentHeightMap = HeightMapGeneration.GenerateHeightmap(65, d, h, depth, seed, zoom: zoom);
        return currentHeightMap;
    }
}
