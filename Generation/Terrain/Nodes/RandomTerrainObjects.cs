using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.GraphCreator;
using JStuff.Generation;
using JStuff.Random;

namespace JStuff.Generation.Terrain
{
    [CreateNodePath("Terrain/Terrain Objects/Random")]
    public class RandomTerrainObjects : TerrainNode
    {
        public List<TerrainObjectType> types;

        InputLink<int> seedInput;
        InputLink<HeightMap> heightmapInput;
        InputLink<List<Vector2>> pointsInput;

        InputLink<float> sizeInput;
        InputLink<float> scaleInput;

        OutputLink<List<TerrainObject>> output;

        public override bool CacheOutput => true;

        protected override void SetupPorts()
        {
            seedInput = AddInputLink<int>();
            heightmapInput = AddInputLink<HeightMap>();
            pointsInput = AddInputLink<List<Vector2>>(portName: "List<Vector2>");

            sizeInput = AddPropertyInputLink<float>("chunkSize");
            scaleInput = AddPropertyInputLink<float>("scale");

            output = AddOutputLink(Evaluate, portName: "TerrainObjects");
        }

        public List<TerrainObject> Evaluate()
        {
            List<TerrainObject> retval = new List<TerrainObject>();
            HeightMap hm = heightmapInput.Evaluate();
            float s = (hm.Length-1) / (sizeInput.Evaluate()-1);
            float scale = scaleInput.Evaluate();

            System.Random rng = new System.Random(seedInput.Evaluate());

            foreach (Vector2 v in pointsInput.Evaluate())
            {
                int index = rng.Next(0, types.Count);
                retval.Add(new TerrainObject(types[index].prefab, new Vector3(v.x, hm[(int)(v.x * s), (int)(v.y * s)] * scale, v.y), types[index].objectRadius));
            }

            return retval;
        }

        public override Node Clone()
        {
            RandomTerrainObjects retval = base.Clone() as RandomTerrainObjects;
            List<TerrainObjectType> nprefabs = new List<TerrainObjectType>();

            foreach (TerrainObjectType i in types)
            {
                TerrainObjectType t = new TerrainObjectType();
                t.prefab = i.prefab;
                t.objectRadius = i.objectRadius;
                nprefabs.Add(t);
            }

            retval.types = nprefabs;
            return retval;
        }
    }
}