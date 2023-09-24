using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.GraphCreator;
using JStuff.Generation;
using System.Linq;
using JStuff.Utilities;
using JStuff.Utility;

namespace JStuff.Generation.Terrain
{
    [CreateNodePath("Terrain/Terrain Objects/Random")]
    public class RandomTerrainObjects : TerrainNode
    {
        InputLink<int> seedInput;
        InputLink<HeightMap> heightmapInput;
        InputLink<TerrainBiome> biomeInput;
        InputMultiLink<List<Vector2>> pointsInput;

        // Property links
        InputLink<float> chunkSizeInput;
        InputLink<float> scaleInput;
        InputLink<Vector2> chunkPositionInput;
        InputLink<int> LODInput;

        public override bool CacheOutput => true;

        protected override void SetupPorts()
        {
            seedInput = AddInputLink<int>();
            heightmapInput = AddInputLink<HeightMap>();

            biomeInput = AddInputLink<TerrainBiome>(portName: "Biome");
            pointsInput = AddInputMultiLink<List<Vector2>>(portName: "List<Vector2>");

            chunkSizeInput = AddPropertyInputLink<float>("chunkSize");
            scaleInput = AddPropertyInputLink<float>("scale");
            chunkPositionInput = AddPropertyInputLink<Vector2>("chunkPosition");
            LODInput = AddPropertyInputLink<int>("terrainObjectLOD");

            AddOutputLink(Evaluate, portName: "TerrainObjects");
        }

        public TerrainObjectCollection Evaluate()
        {
            HeightMap hm = heightmapInput.Evaluate();
            float meshSize = chunkSizeInput.Evaluate();
            float s = (hm.Width - 1) / (chunkSizeInput.Evaluate());
            float heightScale = scaleInput.Evaluate();
            Vector2 chunkPosition = chunkPositionInput.Evaluate();

            int LOD = LODInput.Evaluate();

            TerrainBiome biome = biomeInput.Evaluate();

            List<BiomeLayer> biomeLayers = biome.layers;

            System.Random rng = new System.Random(seedInput.Evaluate());

            List<Vector2>[] pointsList = pointsInput.EvaluateAll();

            List<TerrainObject> terrainObjects = new List<TerrainObject>();
            List<(FoliageObject, List<List<Matrix4x4>>)> foliage = new List<(FoliageObject, List<List<Matrix4x4>>)>();

            for (int i = 0; i < biome.layers.Count; i++)
            {
                BiomeLayer layer = biomeLayers[i];

                if (layer.terrainObjectLOD < LOD || LOD < 0)
                    continue;

                List<TerrainObjectType> weightedPrefabs = layer.terrainObjectTypes;

                List<FoliageObjectType> weightedFoliageObjects = layer.foliageObjectTypes; // TODO

                float prefabWeight = weightedPrefabs.Reduce<TerrainObjectType, float>((x, r) => x.weight + r, 0);
                float foliageWeight = weightedFoliageObjects.Reduce<FoliageObjectType, float>((x, r) => x.weight + r, 0);

                float totalWeight = prefabWeight + foliageWeight;

                prefabWeight /= totalWeight;
                foliageWeight /= totalWeight;

                foreach (Vector2 v in pointsList[i])
                {
                    int x = (int)(v.x * s);
                    int y = (int)(v.y * s);

                    float slope = hm.GetSlope(Mathf.FloorToInt(v.x * s), Mathf.FloorToInt(v.y * s)) * heightScale / meshSize * hm.Width;
                    float height = hm.GetContinousHeight(v.x * s, v.y * s) * heightScale;

                    if (((float)rng.NextDouble()) < prefabWeight)
                    {
                        TerrainObjectType typeToSpawn = weightedPrefabs.Choose((float)rng.NextDouble(), (x) => x.weight); // TODO:PERFORMANCE

                        if (slope > layer.maxSlope || height < layer.minHeight || height > layer.maxHeight)
                            continue;

                        float rot = (float)rng.NextDouble() * 360f;

                        float rot_x = (float)rng.NextDouble();
                        float rot_y = (float)rng.NextDouble();
                        float rot_z = (float)rng.NextDouble();

                        if (typeToSpawn.RotateToTerrainNormal)
                        {
                            // HM normal
                            // sample the height map:
                            //float fx0 = hm[x - 1, y] * heightScale, fx1 = hm[x + 1, y] * heightScale;
                            //float fy0 = hm[x, y - 1] * heightScale, fy1 = hm[x, y + 1] * heightScale;

                            float hmStep = 1 / (hm.Width - 1);

                            float fx0 = hm.GetContinousHeight(x - hmStep, y) * heightScale, fx1 = hm.GetContinousHeight(x + hmStep, y) * heightScale;
                            float fy0 = hm.GetContinousHeight(x, y - hmStep) * heightScale, fy1 = hm.GetContinousHeight(x, y + hmStep) * heightScale;

                            // the spacing of the grid in same units as the height map
                            float eps = meshSize / (hm.Width - 1);

                            // plug into the formulae above:
                            Vector3 normal = (new Vector3((fx0 - fx1) / (2 * eps), (fy0 - fy1) / (2 * eps), 1)).normalized;

                            Vector3 forward = Vector3.Cross(normal, Vector3.forward).normalized;

                            terrainObjects.Add(new TerrainObject(
                                typeToSpawn.prefab,
                                new Vector3(v.x, hm[x, y] * heightScale, v.y),
                                layer.objectRadius,
                                1 + (typeToSpawn.scaleChange + layer.scaleChange) * (1 + (typeToSpawn.scaleVar + layer.scaleVar) * ((float)rng.NextDouble() * 2 - 1)),
                                Quaternion.LookRotation(forward, normal)));
                        }
                        else
                        {
                            terrainObjects.Add(new TerrainObject(
                                typeToSpawn.prefab,
                                new Vector3(v.x, hm[x, y] * heightScale, v.y),
                                layer.objectRadius,
                                1 + (typeToSpawn.scaleChange + layer.scaleChange) * (1 + (typeToSpawn.scaleVar + layer.scaleVar) * ((float)rng.NextDouble() * 2 - 1)),
                                Quaternion.Euler(0, rot, 0)));
                        }
                    } else
                    {
                        FoliageObjectType typeToSpawn = weightedFoliageObjects.Choose((float)rng.NextDouble(), (x) => x.weight); // TODO:PERFORMANCE

                        if (slope > layer.maxSlope || height < layer.minHeight || height > layer.maxHeight)
                            continue;

                        float rot = (float)rng.NextDouble() * 360f;

                        float rot_x = (float)rng.NextDouble();
                        float rot_y = (float)rng.NextDouble();
                        float rot_z = (float)rng.NextDouble();

                        if (typeToSpawn.RotateToTerrainNormal)
                        {
                            // HM normal
                            // sample the height map:
                            //float fx0 = hm[x - 1, y] * heightScale, fx1 = hm[x + 1, y] * heightScale;
                            //float fy0 = hm[x, y - 1] * heightScale, fy1 = hm[x, y + 1] * heightScale;

                            float hmStep = 1 / (hm.Width - 1);

                            float fx0 = hm.GetContinousHeight(x - hmStep, y) * heightScale, fx1 = hm.GetContinousHeight(x + hmStep, y) * heightScale;
                            float fy0 = hm.GetContinousHeight(x, y - hmStep) * heightScale, fy1 = hm.GetContinousHeight(x, y + hmStep) * heightScale;

                            // the spacing of the grid in same units as the height map
                            float eps = meshSize / (hm.Width - 1);

                            // plug into the formulae above:
                            Vector3 normal = (new Vector3((fx0 - fx1) / (2 * eps), (fy0 - fy1) / (2 * eps), 1)).normalized;

                            Vector3 forward = Vector3.Cross(normal, new Vector3(rot_x, rot_y, rot_z)).normalized;

                            Vector3 position = new Vector3(v.x, hm[x, y] * heightScale, v.y) + new Vector3(chunkPosition.x, 0, chunkPosition.y);
                            Vector3 scale = Vector3.one * (1 + (typeToSpawn.scaleChange + layer.scaleChange) * (1 + (typeToSpawn.scaleVar + layer.scaleVar) * ((float)rng.NextDouble() * 2 - 1)));
                            Quaternion rotation = Quaternion.AngleAxis(0, normal);

                            int index = GetFoliageIndex(foliage, typeToSpawn.foliageObject);

                            List<Matrix4x4> batch = foliage[index].Item2[foliage[index].Item2.Count - 1];

                            if (batch.Count < 1000)
                            {

                            } else
                            {
                                foliage[index].Item2.Add(new List<Matrix4x4>());
                                batch = foliage[index].Item2[foliage[index].Item2.Count - 1];
                            }

                            batch.Add(Matrix4x4.TRS(position, rotation, scale));
                        }
                        else
                        {
                            Vector3 position = new Vector3(v.x, hm[x, y] * heightScale, v.y) + new Vector3(chunkPosition.x, 0, chunkPosition.y);
                            Vector3 scale = Vector3.one * (1 + (typeToSpawn.scaleChange + layer.scaleChange) * (1 + (typeToSpawn.scaleVar + layer.scaleVar) * ((float)rng.NextDouble() * 2 - 1)));
                            Quaternion rotation = Quaternion.Euler(0, rot, 0);

                            int index = GetFoliageIndex(foliage, typeToSpawn.foliageObject);

                            var something = foliage[index];
                            List<Matrix4x4> batch = foliage[index].Item2[foliage[index].Item2.Count - 1];

                            if (batch.Count < 1000)
                            {

                            }
                            else
                            {
                                foliage[index].Item2.Add(new List<Matrix4x4>());
                                batch = foliage[index].Item2[foliage[index].Item2.Count - 1];
                            }

                            batch.Add(Matrix4x4.TRS(position, rotation, scale));
                        }
                    }
                }
            }

            return new TerrainObjectCollection(terrainObjects, foliage);
        }

        private int GetFoliageIndex(List<(FoliageObject, List<List<Matrix4x4>>)> foliage, FoliageObject foliageObject)
        {
            if (foliage.Count == 0)
            {
                foliage.Add((foliageObject, new List<List<Matrix4x4>>()));
                foliage[0].Item2.Add(new List<Matrix4x4>());
                return 0;
            }

            for (int i = 0; i < foliage.Count; i++)
            {
                if (foliageObject == foliage[i].Item1)
                {
                    return i;
                }
            }

            foliage.Add((foliageObject, new List<List<Matrix4x4>>()));
            foliage[foliage.Count - 1].Item2.Add(new List<Matrix4x4>());
            return foliage.Count - 1;
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