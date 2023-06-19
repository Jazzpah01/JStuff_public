using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.GraphCreator;
using JStuff.Generation;

namespace JStuff.Generation.Terrain
{
    [CreateNodePath("Terrain/Mesh/Block")]
    public class MeshBlockAtDistance : TerrainNode
    {
        public int coordDistance;

        InputLink<MeshData> input;

        InputLink<float> sizeInput;
        InputLink<Vector2> positionInput;
        InputLink<Vector2> centerPosInput;

        OutputLink<MeshData> output;

        protected override void SetupPorts()
        {
            input = AddInputLink<MeshData>();

            sizeInput = AddPropertyInputLink<float>("chunkSize");
            positionInput = AddPropertyInputLink<Vector2>("chunkPosition");
            centerPosInput = AddPropertyInputLink<Vector2>("centerPosition");

            output = AddOutputLink(Evaluate, portName: "MeshData");
        }
        static int cnt;
        public MeshData Evaluate()
        {
            float size = sizeInput.Evaluate();
            Vector2 position = positionInput.Evaluate();
            Vector2 center = centerPosInput.Evaluate();

            float d0 = Mathf.Abs((float)position.x - (float)center.x);
            float d1 = Mathf.Abs((float)position.y - (float)center.y);
            float j = Mathf.Max(d0, d1);

            int dist = Mathf.RoundToInt(j / size);

            if (dist >= coordDistance)
            {
                return null;
            }
            else
            {
                return input.Evaluate();
            }
        }

        public override Node Clone()
        {
            MeshBlockAtDistance retval = base.Clone() as MeshBlockAtDistance;
            retval.coordDistance = coordDistance;

            return retval;
        }
    }
}