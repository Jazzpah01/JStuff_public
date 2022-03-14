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
        InputLink<float> scaleInput;
        InputLink<HeightMap> heightmapInput;
        InputLink<List<Vector2>> pointsInput;
        OutputLink<List<TerrainObject>> output;

        public override bool CacheOutput => true;

        protected override void SetupPorts()
        {
            seedInput = AddInputLink<int>();
            sizeInput = AddInputLink<float>();
            scaleInput = AddInputLink<float>();
            heightmapInput = AddInputLink<HeightMap>();
            pointsInput = AddInputLink<List<Vector2>>(portName: "List<Vector2>");
            output = AddOutputLink(Evaluate, portName: "TerrainObjects");
        }

        public List<TerrainObject> Evaluate()
        {
            List<TerrainObject> retval = new List<TerrainObject>();
            HeightMap hm = heightmapInput.Evaluate();
            float s = hm.Length / sizeInput.Evaluate();
            float scale = scaleInput.Evaluate();

            float nseed0 = 1.0f / seedInput.Evaluate();
            float nseed1 = Generator.NormalValue(nseed0, 0.512623f);

            foreach (Vector2 v in pointsInput.Evaluate())
            {
                nseed0 = Generator.NormalValue(nseed0, nseed1);
                nseed1 = Generator.NormalValue(nseed0, nseed1);

                int index = (int)Mathf.Clamp(nseed0 * prefabs.Count, 0, prefabs.Count-1);
                retval.Add(new TerrainObject(prefabs[index], new Vector3(v.x, hm[(int)(v.x * s), (int)(v.y * s)] * scale, v.y)));
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