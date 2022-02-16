using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;
using JStuff.Randomness;
using JStuff.Threading;
using System;
using System.Diagnostics;

namespace JStuff.Generation
{
    public class Chunk : MonoBehaviour, IPrioritizedConsumer
    {
        private bool init = false;
        private Material material;
        private MapGenerator mother;

        private int currentSize = 0;
        private int currentDepth = 0;
        [SerializeField] private int currentSkips = 0;
        private Vector3 currentPosition;

        public bool waitingJobResult = false;

        private HeightMap currentHeightMap;
        private HeightMap currentGreenMap;

        [SerializeField] private int heightMapSize;
        [SerializeField] private int lastActualSkips;
        [SerializeField] private int currentDistance;

        private List<GameObject>[] foliage;

        private bool newPosChunk = true;

        private static int ids = 0;
        public int ID = 0;

        private MeshData meshData;
        private MeshData colliderMeshData;
        private Vector3[] actualVertices;
        private Color[] colormap;
        private bool shouldUpdateMeshes = false;
        private bool[] foliagePointsCalculated;
        private List<Vector2>[] foliagePoints;

        private GameObject grassGameObject;
        //private GeometryGrassGenerator grassGeometry;

        public int Priority
        {
            get
            {
                if ((currentPosition - mother.centerChunk).magnitude < mother.chunkSize * 2)
                {
                    return 0;
                }
                return 1;
            }
        }


        //private EquilateralTriangle generator;

        private void Awake()
        {
            ID = ids;
            ids++;
        }
        private void Start()
        {
            //List<Vector2> trees = PointPlacement.GetPoints(mother.chunkSize, 1, 10, 0.2f);
            //trees = mother.climate.FilterFoliage(trees, mother.chunkSize, actual, greenmap, 0);
            if (!init)
            {
                throw new System.Exception("Chunk hasn't been intialized!");
            }
        }

        public void Initialize(MapGenerator mother)
        {
            this.mother = mother;
            gameObject.AddComponent<MeshRenderer>().material = mother.material;
            gameObject.AddComponent<MeshFilter>();
            gameObject.AddComponent<MeshCollider>().enabled = false;

            currentPosition = transform.position;
            //generator = new EquilateralTriangle();

            foliage = new List<GameObject>[mother.climate.foliage.Length];

            grassGameObject = new GameObject("GrassGameObject");
            //grassGeometry = grassGameObject.AddComponent<GeometryGrassGenerator>();
            //grassGeometry.Initialize(this.transform, mother.climate.grassMaterial);

            init = true;
        }

        public void UpdateChunk(int skips, int nextSkips, Vector3 newpos, int distance)
        {
            if (waitingJobResult)
            {
                return;
            }

            bool enableThreading = true;
            int depth = mother.globalDepth;

            if (currentSkips == skips && currentPosition == newpos)
            {
                enableThreading = false;
            }

            if (currentPosition != newpos)
            {
                // This is a new position
                currentSize = 0;
                currentSkips = 0;
                currentHeightMap = null;
                currentDepth = 0;

                foreach (List<GameObject> f in foliage)
                {
                    if (f != null && f.Count != 0)
                    {
                        foreach (GameObject go in f)
                        {
                            Destroy(go);
                        }
                    }
                }
                foliage = new List<GameObject>[mother.climate.foliage.Length];
            }
            else
            {
                // This isn't a new position
                for (int i = 0; i < foliage.Length; i++)
                {
                    if (foliage[i] != null && mother.climate.foliage[i].renderFromDistance < distance)
                    {
                        foreach (GameObject go in foliage[i])
                        {
                            Destroy(go);
                        }
                        foliage[i] = new List<GameObject>();
                    }
                }
            }



            int actualSkips = 0;

            if (currentSkips != skips)
            {
                if (currentSkips == 0 || currentSkips == 1 || currentSkips == -1)
                {
                    actualSkips = skips;
                }
                else
                if (currentSkips < skips)
                {
                    actualSkips = skips / currentSkips;
                }
                else
                {
                    actualSkips = -(currentSkips / skips);
                }
            }

            int size = mother.globalSize;
            currentSize = size;
            currentSkips = skips;
            currentDepth = depth;
            lastActualSkips = actualSkips;

            currentPosition = newpos;

            Vector3 chunkPosition = currentPosition - new Vector3(transform.position.x % mother.chunkSize, 0, transform.position.z % mother.chunkSize);

            float offsetX = mother.offsetX + chunkPosition.x / mother.chunkSize / mother.zoom;
            float offsetZ = mother.offsetZ + chunkPosition.z / mother.chunkSize / mother.zoom;

            if (currentSkips > 1)
            {
                size = (size - 1) / currentSkips + 1;
            }
            else if (currentSkips < -1)
            {
                size = (size - 1) * (-currentSkips) + 1;
            }

            // Set job parameters
            JobParameters p = new JobParameters();
            p.size = size;
            p.absoluteSize = mother.globalSize;
            p.depth = depth;
            p.seed = mother.seed;
            p.zoom = mother.zoom;
            p.offsetX = offsetX;
            p.offsetZ = offsetZ;
            p.map = currentHeightMap;
            p.actualSkips = actualSkips;
            p.absoluteSkips = skips;
            p.nextSkips = nextSkips;
            p.HeightFunction = mother.HeightFunction;
            p.GreenFunction = mother.GreenFunction;
            p.heightMapMultiplier = mother.heightmapMultiplier;
            p.meshHeightMultiplier = mother.meshHeightMultiplier;
            p.chunkSize = mother.chunkSize;
            p.withHM = (currentHeightMap != null);
            p.nonLinearScaling = mother.nonLinearScaling;

            p.treeAmount = 30;
            p.doTrees = true;

            p.climate = mother.climate;

            p.autoCache = mother.autoCache;

            p.centerCunk = mother.centerChunk;

            p.chunkPosition = new Vector2(currentPosition.x, currentPosition.z);

            p.edges = new List<int>();

            p.newPosition = (currentPosition == newpos);

            p.distance = distance;

            p.colliderMeshSkips = mother.colliderSkips;

            if (currentPosition == mother.centerChunk)
            {
                p.edges.Add(0);
                p.edges.Add(1);
                p.edges.Add(2);
                p.edges.Add(3);
            } else
            {
                float xDiff = Mathf.Abs(currentPosition.x - mother.centerChunk.x);
                float zDiff = Mathf.Abs(currentPosition.z - mother.centerChunk.z);

                if (xDiff > zDiff)
                {
                    if (currentPosition.x < mother.centerChunk.x)
                    {
                        p.edges.Add(2);
                    }
                    if (currentPosition.x > mother.centerChunk.x)
                    {
                        p.edges.Add(0);
                    }
                } else if (xDiff < zDiff)
                {
                    if (currentPosition.z < mother.centerChunk.z)
                    {
                        p.edges.Add(3);
                    }
                    if (currentPosition.z > mother.centerChunk.z)
                    {
                        p.edges.Add(1);
                    }
                } else
                {
                    if (currentPosition.x < mother.centerChunk.x)
                    {
                        p.edges.Add(2);
                    }
                    if (currentPosition.x > mother.centerChunk.x)
                    {
                        p.edges.Add(0);
                    }
                    if (currentPosition.z < mother.centerChunk.z)
                    {
                        p.edges.Add(3);
                    }
                    if (currentPosition.z > mother.centerChunk.z)
                    {
                        p.edges.Add(1);
                    }
                }
            }
            waitingJobResult = true;
            //JobManager.GetInstance().AddJob(this, ChunkJob, p);
            //if (enableThreading)
            //{
            //    // Assign job
            //    waitingJobResult = true;
            //    JobManager.GetInstance().AddJob(this, ChunkJob, p);
            //} else
            //{
            //    SimpleMeshUpdate(p);
            //}
        }

        public void SimpleMeshUpdate(JobParameters p)
        {
            if (p.nextSkips > p.absoluteSkips)
            {
                actualVertices = GetEdgedVertices(p, p.map, meshData.vertices);
            } else
            {
                actualVertices = meshData.vertices;
            }
            mother.MeshReady();
        }

        public void MeshUpdate()
        {
            if (!shouldUpdateMeshes)
                return;

            Mesh mesh = new Mesh();

            this.meshData = meshData;

            mesh.vertices = actualVertices;
            mesh.uv = meshData.uv;
            mesh.triangles = meshData.triangles;
            mesh.colors = colormap;
            mesh.RecalculateNormals();

            gameObject.GetComponent<MeshFilter>().mesh = mesh;

            transform.position = currentPosition;

            Vector3 chunkPosition = transform.position -
                new Vector3(transform.position.x % mother.chunkSize, 0, transform.position.z % mother.chunkSize);

            if ((chunkPosition - mother.centerChunk).magnitude < mother.colliderOnDistance * mother.chunkSize)
            {
                Mesh colliderMesh = new Mesh();

                colliderMesh.vertices = colliderMeshData.vertices;
                colliderMesh.triangles = colliderMeshData.triangles;

                MeshCollider mc = gameObject.GetComponent<MeshCollider>();
                mc.enabled = true;
                mc.sharedMesh = colliderMesh;
            }
            else
            {
                gameObject.GetComponent<MeshCollider>().enabled = false;
            }
        }

        public object ChunkJob(object parameters)
        {
            JobParameters p = (JobParameters)parameters;

            HeightMap h = null;

            HeightMap greenmap = HeightMapGeneration.GenerateHeightmap(p.GreenFunction, p.size / 2, depth: 9, seed: p.seed * p.seed,
                    zoom: p.zoom, offsetX: p.offsetX, offsetZ: p.offsetZ,
                    nonLinearScaling: true);

            greenmap = greenmap.FilteredHeightMap(x => (x < 0) ? -x : x);
            greenmap *= p.climate.greenFactor;

            float[,] greenmapAlt = new float[greenmap.Width, greenmap.Width];

            for (int i = 0; i < greenmap.Width; i++)
            {
                for (int j = 0; j < greenmap.Width; j++)
                {
                    greenmapAlt[i, j] = greenmap[i, j] - greenmap[i, j] * mother.RoadInfluence(new Vector2(i, j) / (float)greenmap.Width * p.chunkSize + p.chunkPosition);
                }
            }

            greenmap = new HeightMap(greenmapAlt);

            if (p.withHM)
            {
                if (p.actualSkips < 1)
                {
                    h = HeightMapGeneration.GenerateHeightmap(p.HeightFunction, p.map, p.actualSkips, depth: p.depth,
                        seed: p.seed, zoom: p.zoom, offsetX: p.offsetX, offsetZ: p.offsetZ,
                        nonLinearScaling: p.nonLinearScaling, autoCache: p.autoCache);
                } else if (p.actualSkips > 1)
                {
                    h = p.map.Simplified(p.actualSkips);
                } else
                {
                    h = p.map;
                }
            }
            else
            {
                h = HeightMapGeneration.GenerateHeightmap(p.HeightFunction, p.size, depth: p.depth, seed: p.seed,
                    zoom: p.zoom, offsetX: p.offsetX, offsetZ: p.offsetZ,
                    nonLinearScaling: p.nonLinearScaling, autoCache: p.autoCache);
            }

            HeightMap actual = h * p.heightMapMultiplier;

            MeshData meshData = TerrainMeshGeneration.GenerateMesh(actual, p.chunkSize, p.meshHeightMultiplier);
            Vector3[] actualVertices = meshData.vertices;

            MeshData colliderMeshData;

            if (p.colliderMeshSkips == p.absoluteSkips ||
                !(p.colliderMeshSkips > 1 || p.colliderMeshSkips < -1) &&
                !(p.absoluteSkips > 1 || p.absoluteSkips < -1))
            {
                colliderMeshData = meshData;
            } else
            {
                if (p.colliderMeshSkips > p.absoluteSkips)
                {
                    colliderMeshData = TerrainMeshGeneration.GenerateMesh(actual.Simplified(p.colliderMeshSkips / p.absoluteSkips), p.chunkSize, p.meshHeightMultiplier);
                }
                else
                {
                    colliderMeshData = TerrainMeshGeneration.GenerateMesh(HeightMapGeneration.GenerateHeightmap(p.HeightFunction, actual, -(p.absoluteSkips / p.colliderMeshSkips), depth: p.depth,
                         seed: p.seed, zoom: p.zoom, offsetX: p.offsetX, offsetZ: p.offsetZ,
                         nonLinearScaling: p.nonLinearScaling, autoCache: p.autoCache), p.chunkSize, p.meshHeightMultiplier);
                }
            }

            if (p.nextSkips > p.absoluteSkips)
            {
                actualVertices = GetEdgedVertices(p, h, meshData.vertices);
            }

            Color[] colormap = mother.climate.GetColormap(actual, greenmap, p.meshHeightMultiplier / (p.zoom * p.chunkSize));

            // Trees
            List<Vector2>[] trees = new List<Vector2>[p.climate.foliage.Length];
            for (int i = 0; i < p.climate.foliage.Length; i++)
            {
                //float r = (Mathf.Abs(h[0, 0]) * (1 / (i + 1) * 100 * Mathf.PI)).FractionalDigits();
                float r = (Mathf.Abs(h[0, 0]) * p.climate.foliage[i].seed).FractionalDigits();
                trees[i] = PointPlacement.GetPoints(p.chunkSize, p.climate.foliage[i].radius, p.climate.foliage[i].amount, r);
                trees[i] = p.climate.FilterFoliage(trees[i], p.chunkSize, actual, greenmap, p.meshHeightMultiplier / p.zoom / p.chunkSize, 0);
            }

            // Grass
            //GeometryGrassGenerator.GrassMeshData grassMeshData = new GeometryGrassGenerator.GrassMeshData();
            //List<Vector2> grassPoints = PointPlacement.GetPoints(1, p.climate.grassRadious, Mathf.FloorToInt(p.climate.grassprSquareM * p.chunkSize * p.chunkSize), 
            //    RandomGenerator.NormalValue(greenmap[0, 0], h[0, 0]), true);
            //if (grassPoints.Count > 0)
            //{
            //    grassPoints = p.climate.FilterGrassPoints(grassPoints, colormap, actual.Width);
            //}
            //grassMeshData = GeometryGrassGenerator.GenerateGrassMeshData(actual, grassPoints, Vector3.zero, p.chunkSize, p.meshHeightMultiplier);

            ConsumeParameters retval = new ConsumeParameters();

            retval.h = h;
            retval.meshData = meshData;
            retval.colliderMeshData = colliderMeshData;
            retval.actualVertices = actualVertices;
            retval.foliagePoints = trees;
            retval.p = p;
            retval.colormap = colormap;
            //retval.grassMeshData = grassMeshData;

            return retval;
        }

        public Vector3[] GetEdgedVertices(JobParameters p, HeightMap h, Vector3[] originalVertices)
        {
            int edgeSkips = p.nextSkips / p.absoluteSkips;
            Vector3[] retval = new Vector3[h.Width * h.Width];
            Array.Copy(originalVertices, 0, retval, 0, h.Width * h.Width);

            foreach (int edge in p.edges)
            {
                for (int i = 0; i < h.Width; i++)
                {
                    int x = 0, y = 0, lower = 0, upper = 0, current = 0;

                    switch (edge)
                    {
                        case 0:
                            x = h.Width - 1;
                            y = i;
                            lower = x + (y - (y % edgeSkips)) * h.Width;
                            upper = x + (y - (y % edgeSkips) + edgeSkips) * h.Width;
                            current = x + y * h.Width;
                            break;
                        case 1:
                            x = i;
                            y = h.Width - 1;
                            lower = (x - (x % edgeSkips)) + y * h.Width;
                            upper = (x - (x % edgeSkips) + edgeSkips) + y * h.Width;
                            current = x + y * h.Width;
                            break;
                        case 2:
                            x = 0;
                            y = i;
                            lower = x + (y - (y % edgeSkips)) * h.Width;
                            upper = x + (y - (y % edgeSkips) + edgeSkips) * h.Width;
                            current = x + y * h.Width;
                            break;
                        case 3:
                            x = i;
                            y = 0;
                            lower = (x - (x % edgeSkips)) + y * h.Width;
                            upper = (x - (x % edgeSkips) + edgeSkips) + y * h.Width;
                            current = x + y * h.Width;
                            break;
                    }

                    if (i % edgeSkips != 0)
                    {
                        retval[current].y =
                            retval[lower].y +
                            (retval[upper] - retval[lower]).y / edgeSkips * (i % edgeSkips) + 0.1f;
                    }
                }
            }
            return retval;
        }

        public void ConsumeJob(object data)
        {
            StartCoroutine(ConsumeJobCoroutine(data));
        }

        public IEnumerator ConsumeJobCoroutine(object data)
        {
            //(HeightMap h, MeshData meshData, Vector3[] actualVertices, Color[] colormap, List<Vector2>[] fpoints, JobParameters p) =
            //    ((HeightMap, MeshData, Vector3[], Color[], List<Vector2>[], JobParameters))data;

            ConsumeParameters cp = (ConsumeParameters)data;

            //Stopwatch watch = new Stopwatch();
            //watch.Start();
            //
            //Debug.Log("Updating meshes. (Chunk)");

            this.meshData = cp.meshData;
            this.colliderMeshData = cp.colliderMeshData;
            this.actualVertices = cp.actualVertices;
            this.colormap = cp.colormap;
            currentHeightMap = cp.h;

            currentDistance = cp.p.distance;

            if (cp.p.absoluteSize != cp.p.nextSkips || cp.p.newPosition)
            {
                shouldUpdateMeshes = true;
            } else
            {
                shouldUpdateMeshes = false;
            }

            if (!mother.synchronousMeshUpdate)
            {
                MeshUpdate();
            } else
            {
                mother.MeshReady();
            }

            //watch.Stop();
            //
            //Debug.Log("Time taken: " + watch.ElapsedMilliseconds);

            yield return null;

            // Add Grass
            //grassGameObject.transform.position = this.transform.position;

            //if (mother.climate.grassOnDistance >= cp.p.distance)
            //{
            //    if (cp.p.newPosition)
            //    {
            //        if (grassGeometry.ContainsGrass)
            //            grassGeometry.ClearMesh();

            //        grassGeometry.GenerateMesh(cp.grassMeshData);
            //    } else if (!grassGeometry.ContainsGrass)
            //    {
            //        grassGeometry.GenerateMesh(cp.grassMeshData);
            //    }
            //} else if (grassGeometry.ContainsGrass)
            //{
            //    grassGeometry.ClearMesh();
            //}

            yield return null;

            // Add foliage
            for (int i = 0; i < foliage.Length; i++)
            {
                if (foliage[i] == null)
                    foliage[i] = new List<GameObject>();

                if (foliage[i].Count == 0 && cp.p.distance <= mother.climate.foliage[i].renderFromDistance)
                {
                    // No foliage, but needs foliage

                    int fIndex = 0;

                    foreach (Vector2 v in cp.foliagePoints[i])
                    {
                        fIndex++;
                        GameObject go = Instantiate(mother.climate.foliage[i].foliagePrefabs[fIndex % mother.climate.foliage[i].foliagePrefabs.Length]);
                        float x = v.x + currentPosition.x;
                        float z = v.y + currentPosition.z;
                        go.transform.position = new Vector3(x, HeightAtPoint(x, z), z);
                        go.transform.parent = this.transform;

                        float r1 = x - Mathf.Floor(x);
                        float r2 = z - Mathf.Floor(z);

                        go.transform.eulerAngles = new Vector3(0, RandomGenerator.NormalValue(r1, r2) * 360, 0);

                        go.transform.localScale = Vector3.one *
                            (mother.climate.foliage[i].defaultScale +
                             RandomGenerator.Value(r1, r2) * mother.climate.foliage[i].scalevariance);

                        foliage[i].Add(go);
                    }
                }
                else if (cp.p.distance <= mother.climate.foliage[i].renderFromDistance)
                {
                    // Have foliage, but needs position update
                    // This is bad, would be better to make sure it is deterministically deternimned regardless of skips
                    // TODO: improve this
                    foreach (GameObject go in foliage[i])
                    {
                        float x = go.transform.position.x;
                        float z = go.transform.position.z;
                        go.transform.position = new Vector3(x, HeightAtPoint(x, z), z);
                    }
                }
                yield return null;
            }

            waitingJobResult = false;
            yield return true;
        }

        public void JobFailed()
        {
            waitingJobResult = false;
            Debug.Log("Job failed!");
        }

        public struct JobParameters
        {
            public bool withHM;
            public Func<Vertex, Vertex, float, float> HeightFunction;
            public Func<Vertex, Vertex, float, float> GreenFunction;
            public HeightMap map;
            public int actualSkips;
            public int nextSkips;
            public int absoluteSkips;
            public int size;
            public int absoluteSize;
            public int depth;
            public int seed;
            public float zoom;
            public float offsetX;
            public float offsetZ;
            public bool nonLinearScaling;
            public float heightMapMultiplier;
            public float meshHeightMultiplier;
            public float chunkSize;
            public Vector2 chunkPosition;

            public int treeAmount;
            public bool doTrees;

            public Subclimate climate;

            public Vector3 centerCunk;

            public bool autoCache;

            public List<int> edges;

            public bool newPosition;

            public int distance;

            public int colliderMeshSkips;
        }

        public struct ConsumeParameters
        {
            public JobParameters p;
            public HeightMap h;
            public MeshData meshData;
            public MeshData colliderMeshData;
            public Vector3[] actualVertices;
            public Color[] colormap;
            public List<Vector2>[] foliagePoints;
            //public GeometryGrassGenerator.GrassMeshData grassMeshData;
        }

        public float HeightAtPoint(float x, float z)
        {
            int ax = Mathf.FloorToInt(((x - currentPosition.x) / mother.chunkSize * currentHeightMap.Width));
            int az = Mathf.FloorToInt(((z - currentPosition.z) / mother.chunkSize * currentHeightMap.Width));

            float h = 0;
            int i = 0;

            try
            {
                h = currentHeightMap[ax, az];
            } catch
            {
                throw new System.Exception("Cannot get height from point. x: " + ax + ". z: " + az);
            }

            return h * mother.meshHeightMultiplier * mother.heightmapMultiplier;
        }
    }



    /*
     * 
    // Add trees
            if (foliage.Count <= 0)
            {
                foreach (Vector2 tree in trees)
                {
                    if (currentSkips <= 1)
                    {
                        GameObject go = Instantiate(mother.treePrefab);
                        float x = tree.x + currentPosition.x;
                        float z = tree.y + currentPosition.z;
                        go.transform.position = new Vector3(x, HeightAtPoint(x, z), z);
                        go.transform.parent = this.transform;
                    }
                }
            } else
            {
                if (currentSkips > 1)
                {
                    foreach (GameObject go in foliage)
                    {
                        Destroy(go);
                    }
                    foliage = new List<GameObject>();
                } else
                {
                    foreach (GameObject tree in foliage)
                    {
                        tree.transform.position =
                            new Vector3(tree.transform.position.x,
                            HeightAtPoint(tree.transform.position.x, tree.transform.position.z),
                            tree.transform.position.z);
                    }
                }
            }
    */

/*switch (edge)
                    {
                        case 0:
                            x = h.Width - 1;
                            y = i;
                            lower = x + (y-(y % edgeSkips)) * h.Width;
                            upper = x + (y-(y % edgeSkips)+edgeSkips) * h.Width;
                            current = x + y * h.Width;
                            //Debug.Log("Here: " + edgeSkips);
                            //Debug.Log((y - (y % edgeSkips)));
                            //Debug.Log((y - (y % edgeSkips) + edgeSkips));
                            //Debug.Log("To: " + y);
                            break;
                        case 1:
                            x = i;
                            y = h.Width - 1;
                            lower = (x - (x % edgeSkips)) + y * h.Width;
                            upper = (x - (x % edgeSkips)+edgeSkips) + y * h.Width;
                            current = x + y * h.Width;
                            //Debug.Log("Here: " + edgeSkips);
                            //Debug.Log((x - (x % edgeSkips)));
                            //Debug.Log((x - (x % edgeSkips) + edgeSkips));
                            //Debug.Log("To: " + x);
                            break;
                        case 2:
                            x = 0;
                            y = i;
                            lower = x + (y - (y % edgeSkips)) * h.Width;
                            upper = x + (y - (y % edgeSkips) + edgeSkips) * h.Width;
                            current = x + y * h.Width;
                            //Debug.Log("Here: " + edgeSkips);
                            //Debug.Log((y - (y % edgeSkips)));
                            //Debug.Log((y - (y % edgeSkips) + edgeSkips));
                            //Debug.Log("To: " + y);
                            break;
                        case 3:
                            x = i;
                            y = 0;
                            lower = (x - (x % edgeSkips)) + y * h.Width;
                            upper = (x - (x % edgeSkips) + edgeSkips) + y * h.Width;
                            current = x + y * h.Width;
                            //Debug.Log("Here: " + edgeSkips);
                            //Debug.Log((x - (x % edgeSkips)));
                            //Debug.Log((x - (x % edgeSkips) + edgeSkips));
                            //Debug.Log("To: " + x);
                            break;
                    }*/
}
 