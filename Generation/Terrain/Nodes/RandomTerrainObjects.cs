using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.GraphCreator;
using JStuff.Generation;
using JStuff.Random;

namespace JStuff.Generation.Terrain
{
    public class RandomTerrainObjects : GenerateTerrainObjects
    {
        public List<GameObject> prefabs;

        InputLink<int> seedInput;
        InputLink<float> sizeInput;
        InputLink<HeightMap> heightmapInput;
        InputLink<List<Vector2>> pointsInput;
        OutputLink<List<TerrainObject>> output;

        public override bool CacheOutput => true;

        protected override void SetupPorts()
        {
            seedInput = AddInputLink<int>();
            sizeInput = AddInputLink<float>();
            heightmapInput = AddInputLink<HeightMap>();
            pointsInput = AddInputLink<List<Vector2>>();
            output = AddOutputLink(Evaluate);
        }

        public List<TerrainObject> Evaluate()
        {
            List<TerrainObject> retval = new List<TerrainObject>();
            HeightMap hm = heightmapInput.Evaluate();

            foreach (Vector2 v in pointsInput.Evaluate())
            {
                float scale = sizeInput.Evaluate() / hm.Length;

                int index = (int)Mathf.Clamp(Generator.NormalValue(1.0f / seedInput.Evaluate(), 0.512623f) * prefabs.Count, 0, prefabs.Count);
                retval.Add(new TerrainObject(prefabs[index], new Vector3(v.x, hm[(int)(v.x * scale), (int)(v.y * scale)], v.y)));
            }

            return retval;
        }

        public override Node Clone()
        {
            RandomTerrainObjects retval = base.Clone() as RandomTerrainObjects;
            List<GameObject> nprefabs = new List<GameObject>();

            foreach (GameObject i in prefabs)
            {
                nprefabs.Add(i);
            }

            retval.prefabs = nprefabs;
            return retval;
        }
    }
}