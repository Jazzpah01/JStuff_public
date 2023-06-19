using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.GraphCreator;
using JStuff.Generation;
using System.Linq;
using JStuff.Utilities;

namespace JStuff.Generation.Terrain
{
    [CreateNodePath("Terrain/Terrain Objects/Random")]
    public class RandomTerrainObjects : TerrainNode
    {
        InputLink<int> seedInput;
        InputLink<HeightMap> heightmapInput;
        InputLink<List<IWeightedPrefab>> biomeInput;
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

            biomeInput = AddInputLink<List<IWeightedPrefab>>(portName: "Biome");

            sizeInput = AddPropertyInputLink<float>("chunkSize");
            scaleInput = AddPropertyInputLink<float>("scale");

            output = AddOutputLink(Evaluate, portName: "TerrainObjects");
        }

        public List<TerrainObject> Evaluate()
        {
            List<TerrainObject> retval = new List<TerrainObject>();
            HeightMap hm = heightmapInput.Evaluate();
            float meshSize = sizeInput.Evaluate();
            float s = (hm.Width - 1) / (sizeInput.Evaluate());
            float scale = scaleInput.Evaluate();

            List<IWeightedPrefab> weightedPrefabs = biomeInput.Evaluate();

            System.Random rng = new System.Random(seedInput.Evaluate());

            var ssssss = pointsInput.Evaluate();
            int sssss = pointsInput.Evaluate().Count;

            foreach (Vector2 v in pointsInput.Evaluate())
            {
                int x = (int)(v.x * s);
                int y = (int)(v.y * s);

                float slope = hm.GetSlope(Mathf.FloorToInt(v.x * s), Mathf.FloorToInt(v.y * s)) * scale / meshSize * hm.Width;
                float height = hm.GetContinousHeight(v.x * s, v.y * s) * scale;

                TerrainObjectType typeToSpawn = (TerrainObjectType) weightedPrefabs.GetWeightedPrefab(rng);

                if (slope > typeToSpawn.maxSlope || height < typeToSpawn.minHeight || height > typeToSpawn.maxHeight)
                    continue;

                float rot = (float)rng.NextDouble() * 360f;

                retval.Add(new TerrainObject(
                    typeToSpawn.prefab, 
                    new Vector3(v.x, hm[x, y] * scale, v.y), 
                    typeToSpawn.objectRadius,
                    1 + typeToSpawn.scaleChange * (1 + typeToSpawn.scaleVar * ((float)rng.NextDouble()*2-1)),
                    Quaternion.Euler(0, rot, 0)));
            }

            return retval;
        }

        public override Node Clone()
        {
            RandomTerrainObjects retval = base.Clone() as RandomTerrainObjects;

            return retval;
        }
    }
}

//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using JStuff.GraphCreator;
//using JStuff.Generation;
//using JStuff.Random;

//namespace JStuff.Generation.Terrain
//{
//    [CreateNodePath("Terrain/Terrain Objects/Random")]
//    public class RandomTerrainObjects : TerrainNode
//    {
//        public List<TerrainObjectType> types;

//        InputLink<int> seedInput;
//        InputLink<HeightMap> heightmapInput;
//        InputLink<List<Vector2>> pointsInput;

//        InputLink<float> sizeInput;
//        InputLink<float> scaleInput;

//        OutputLink<List<TerrainObject>> output;

//        public override bool CacheOutput => true;

//        protected override void SetupPorts()
//        {
//            seedInput = AddInputLink<int>();
//            heightmapInput = AddInputLink<HeightMap>();
//            pointsInput = AddInputLink<List<Vector2>>(portName: "List<Vector2>");

//            sizeInput = AddPropertyInputLink<float>("chunkSize");
//            scaleInput = AddPropertyInputLink<float>("scale");

//            output = AddOutputLink(Evaluate, portName: "TerrainObjects");
//        }

//        public List<TerrainObject> Evaluate()
//        {
//            List<TerrainObject> retval = new List<TerrainObject>();
//            HeightMap hm = heightmapInput.Evaluate();
//            float meshSize = sizeInput.Evaluate();
//            float s = (hm.Width-1) / (meshSize-1);
//            float scale = scaleInput.Evaluate();

//            System.Random rng = new System.Random(seedInput.Evaluate());

//            foreach (Vector2 v in pointsInput.Evaluate())
//            {
//                int x = (int)(v.x * s);
//                int y = (int)(v.y * s);
//                float slope = hm.GetContinousHeight(v.x * s, v.y * s) * scale / hm.Width * meshSize;
//                int index = rng.Next(0, types.Count);
//                float height = hm[x, y];
//                if (slope > types[index].maxSlope)// || height < types[index].minHeight || height > types[index].maxHeight)
//                    continue;

//                retval.Add(new TerrainObject(types[index].prefab, new Vector3(v.x, hm[x, y] * scale, v.y), types[index].objectRadius));
//            }

//            return retval;
//        }

//        public override Node Clone()
//        {
//            RandomTerrainObjects retval = base.Clone() as RandomTerrainObjects;
//            List<TerrainObjectType> nprefabs = new List<TerrainObjectType>();

//            foreach (TerrainObjectType i in types)
//            {
//                TerrainObjectType t = new TerrainObjectType();
//                t.prefab = i.prefab;
//                t.objectRadius = i.objectRadius;
//                nprefabs.Add(t);
//            }

//            retval.types = nprefabs;
//            return retval;
//        }
//    }
//}