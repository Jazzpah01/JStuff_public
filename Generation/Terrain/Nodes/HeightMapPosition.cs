using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.GraphCreator;

namespace JStuff.Generation.Terrain
{
    public class HeightMapPosition : GenerateHeightMap
    {
        InputLink<float> zoom;
        InputLink<float> chunkSize;
        InputLink<Vector2> chunkPosition;
        OutputLink<Vector2> output;

        protected override void SetupPorts()
        {
            zoom = AddPropertyInputLink("zoom") as InputLink<float>;
            chunkSize = AddPropertyInputLink("chunkSize") as InputLink<float>;
            chunkPosition = AddPropertyInputLink("chunkPosition") as InputLink<Vector2>;
            output = AddOutputLink<Vector2>(Evaluate, portName: "Vector2");
        }

        private Vector2 Evaluate()
        {
            return chunkPosition.Evaluate() / (chunkSize.Evaluate() * zoom.Evaluate());
        }
    }
}