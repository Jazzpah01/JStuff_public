using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.GraphCreator;
using JStuff.Utilities;
using JStuff.Collections;
using System.Runtime.InteropServices.WindowsRuntime;

namespace JStuff.Generation.Terrain
{
    [CreateNodePath("Terrain/Terrain Objects/Layer Mask")]
    public class TerrainObjectLayerMask : TerrainNode
    {
        [Range(0,1)]
        public float randomness = 0;

        InputLink<List<Vector2>> pointsInput;
        InputLink<HeightMap> heightmapInput;
        InputMultiLink<List<IWeightedPrefab>> biome0Input;
        InputMultiLink<HeightMap> layerMaskInput;

        InputLink<int> seedInput;

        InputLink<float> sizeInput;
        InputLink<float> scaleInput;

        protected override void SetupPorts()
        {
            pointsInput = AddInputLink<List<Vector2>>();
            heightmapInput = AddInputLink<HeightMap>();
            biome0Input = AddInputMultiLink<List<IWeightedPrefab>>();
            layerMaskInput = AddInputMultiLink<HeightMap>(portName: "HeightMap(LayerMask)");

            sizeInput = AddPropertyInputLink<float>("chunkSize");
            scaleInput = AddPropertyInputLink<float>("scale");
            seedInput = AddPropertyInputLink<int>("seed");

            AddOutputLink<List<TerrainObject>>(Evaluate);
        }

        List<TerrainObject> Evaluate()
        {
            List<TerrainObject> retval = new List<TerrainObject>();

            HeightMap hm = heightmapInput.Evaluate();
            float meshSize = sizeInput.Evaluate();
            float s = (hm.Width - 1) / (sizeInput.Evaluate());
            float scale = scaleInput.Evaluate();

            System.Random rng = new System.Random(seedInput.Evaluate());

            HeightMap[] layerMask = layerMaskInput.EvaluateAll();

            List<IWeightedPrefab>[] biome = biome0Input.EvaluateAll();

            foreach (Vector2 v in pointsInput.Evaluate())
            {
                int x = (int)(v.x * s);
                int y = (int)(v.y * s);

                float slope = hm.GetSlope(Mathf.FloorToInt(v.x * s), Mathf.FloorToInt(v.y * s)) * scale / meshSize * hm.Width;
                float height = hm.GetContinousHeight(v.x * s, v.y * s) * scale;

                float p = -1;

                TerrainObjectType typeToSpawn = (TerrainObjectType)biome[0].GetWeightedPrefab(rng);

                for (int i = 0; i < layerMaskInput.outputLinks.Count; i++)
                {
                    p = layerMask[i].GetContinousHeight(v.x * s, v.y * s) + ((float)rng.NextDouble()).Remap(0f, 1f, -1f, 1f) * randomness;

                    typeToSpawn = (p > 0) ? (TerrainObjectType)biome[i+1].GetWeightedPrefab(rng) : typeToSpawn;
                }

                if (slope > typeToSpawn.maxSlope || height < typeToSpawn.minHeight || height > typeToSpawn.maxHeight)
                    continue;

                float rot = (float)rng.NextDouble() * 360f;

                retval.Add(new TerrainObject(
                    typeToSpawn.prefab,
                    new Vector3(v.x, hm[x, y] * scale, v.y),
                    typeToSpawn.objectRadius,
                    1 + typeToSpawn.scaleChange * (1 + typeToSpawn.scaleVar * ((float)rng.NextDouble() * 2 - 1)),
                    Quaternion.Euler(0, rot, 0)));
            }

            return retval;
        }

        public override Node Clone()
        {
            var retval = base.Clone() as TerrainObjectLayerMask;
            retval.randomness = randomness;
            return retval;
        }


    }
}