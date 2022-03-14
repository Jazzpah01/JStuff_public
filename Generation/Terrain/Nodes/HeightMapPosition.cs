using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.GraphCreator;

namespace JStuff.Generation.Terrain
{
    public class HeightMapPosition : GenerateHeightMap
    {
        PropertyPort<float> zoom;
        PropertyPort<float> chunkSize;
        PropertyPort<Vector2> chunkPosition;
        OutputLink<Vector2> output;

        protected override void SetupPorts()
        {
            zoom = AddPropertyLink("zoom", false)                     as PropertyPort<float>;
            chunkSize = AddPropertyLink("chunkSize", false)           as PropertyPort<float>;
            chunkPosition = AddPropertyLink("chunkPosition", false)   as PropertyPort<Vector2>;
            output = AddOutputLink<Vector2>(Evaluate, portName: "Vector2");
        }

        private Vector2 Evaluate()
        {
            return chunkPosition.Evaluate() / (chunkSize.Evaluate() * zoom.Evaluate());
        }
    }
}