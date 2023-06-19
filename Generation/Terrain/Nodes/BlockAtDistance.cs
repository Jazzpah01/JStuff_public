using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.GraphCreator;
using JStuff.Generation;

namespace JStuff.Generation.Terrain
{
    [CreateNodePath("Terrain/Terrain Objects/LOD")]
    public class BlockAtDistance : TerrainNode
    {
        public int coordDistance;

        InputLink<List<TerrainObject>> input;

        InputLink<float> sizeInput;
        InputLink<Vector2> positionInput;
        InputLink<Vector2> centerPosInput;

        OutputLink<List<TerrainObject>> output;

        //public override bool CacheOutput => true;

        protected override void SetupPorts()
        {
            input = AddInputLink<List<TerrainObject>>();

            sizeInput = AddPropertyInputLink<float>("chunkSize");
            positionInput = AddPropertyInputLink<Vector2>("chunkPosition");
            centerPosInput = AddPropertyInputLink<Vector2>("centerPosition");

            output = AddOutputLink(Evaluate, portName: "TerrainObjects");
        }
        static int cnt;
        public List<TerrainObject> Evaluate()
        {
            float size = sizeInput.Evaluate();
            Vector2 position = positionInput.Evaluate();
            Vector2 center = centerPosInput.Evaluate();

            float d0 = Mathf.Abs((float)position.x - (float)center.x);
            float d1 = Mathf.Abs((float)position.y - (float)center.y);
            float j = Mathf.Max(d0, d1);

            int dist = Mathf.RoundToInt(j / size);

            if (dist > coordDistance)
            {
                return new List<TerrainObject>();
            } else
            {
                return input.Evaluate();
            }
        }

        public override Node Clone()
        {
            BlockAtDistance retval = base.Clone() as BlockAtDistance;
            retval.coordDistance = coordDistance;

            return retval;
        }
    }
}