using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.GraphCreator;
using JStuff.Generation;

namespace JStuff.Generation.Terrain
{
    [CreateNodePath("Terrain/Utility/Distance to center")]
    public class DistanceToCenter : TerrainNode
    {
        InputLink<float> sizeInput;
        InputLink<Vector2> positionInput;
        InputLink<Vector2> centerPosInput;

        OutputLink<int> output;

        public override bool CacheOutput => true;

        protected override void SetupPorts()
        {
            sizeInput = AddPropertyInputLink<float>("chunkSize");
            positionInput = AddPropertyInputLink<Vector2>("chunkPosition");
            centerPosInput = AddPropertyInputLink<Vector2>("centerPosition");

            output = AddOutputLink(Evaluate);
        }

        public int Evaluate()
        {
            float size = sizeInput.Evaluate();
            Vector2 position = positionInput.Evaluate();
            Vector2 center = centerPosInput.Evaluate();

            return Mathf.RoundToInt(Mathf.Max(Mathf.Abs(position.x - center.x), Mathf.Abs(position.y - center.y)) / size);
        }

        public override Node Clone()
        {
            DistanceToCenter retval = base.Clone() as DistanceToCenter;

            return retval;
        }
    }
}