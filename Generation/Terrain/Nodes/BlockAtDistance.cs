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
        public int LOD;

        InputLink<List<TerrainObject>> input;

        InputLink<float> sizeInput;
        InputLink<Vector2> positionInput;
        InputLink<Vector2> centerPosInput;
        InputLink<int> lodInput;

        OutputLink<List<TerrainObject>> output;

        //public override bool CacheOutput => true;

        protected override void SetupPorts()
        {
            input = AddInputLink<List<TerrainObject>>();

            sizeInput = AddPropertyInputLink<float>("chunkSize");
            positionInput = AddPropertyInputLink<Vector2>("chunkPosition");
            centerPosInput = AddPropertyInputLink<Vector2>("centerPosition");

            lodInput = AddPropertyInputLink<int>("lod");

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

            if (lodInput.Evaluate() > LOD)
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
            retval.LOD = LOD;

            return retval;
        }
    }
}