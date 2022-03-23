using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.GraphCreator;

namespace JStuff.Generation.Terrain
{
    [CreateNodePath("Terrain/Utility/HeightMap Position")]
    public class HeightMapPosition : TerrainNode
    {
        InputLink<float> zoom;
        InputLink<float> chunkSize;
        InputLink<Vector2> chunkPosition;
        OutputLink<Vector2> output;

        protected override void SetupPorts()
        {
            zoom = AddPropertyInputLink<float>("zoom");
            chunkSize = AddPropertyInputLink<float>("chunkSize");
            chunkPosition = AddPropertyInputLink<Vector2>("chunkPosition");
            output = AddOutputLink<Vector2>(Evaluate, portName: "Vector2");
        }

        private Vector2 Evaluate()
        {
            return chunkPosition.Evaluate() / (chunkSize.Evaluate() * zoom.Evaluate());
        }
    }
}