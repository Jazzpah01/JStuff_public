using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using UnityEngine.Profiling;
using JStuff.Threading;
using JStuff.Randomness;
using JStuff.AI.Pathfinding;

namespace JStuff.Generation
{
    public class MapGenerator : MonoBehaviour
    {
        public GameObject viewer;

        //[SerializeReference]
        public Subclimate climate;

        public GameObject treePrefab;

        public int chunkAmount = 3;
        public float chunkSize = 10;
        public Func<Vertex, Vertex, float, float> heightFunction;

        public float offsetX = 0;
        public float offsetZ = 0;
        public float zoom = 1;
        public int seed = 0;

        public bool synchronousMeshUpdate = false;

        public bool autoCache = false;
        public int cacheAtDepth = -1;

        public int globalSize;
        public int globalDepth;
        public List<int> skips;

        public int greenMapSize;
        public int greenMapDepth;

        public Material material;

        public float meshHeightMultiplier = 1;
        public float heightmapMultiplier = 1;

        public int colliderOnDistance = 2;
        public int colliderSkips = 1;

        public bool nonLinearScaling = false;

        public Gradient colorHeightGradient;
        public Gradient colorSlopeGradient;
        public InterpolateFunction slopeInterpolate;
        public Gradient grayScaleGradient;

        public float distanceFactor;
        public float heightDifferenceFactor;

        public InterpolateFunction distanceToDepthMultiplier;

        private Chunk[] chunks;
        private GameObject parentChunk;
        [HideInInspector] public Vector3 centerChunk;
        public Queue<Vector3> newPositions;
        private Vector3 updatedPlayerPosition;

        private bool init = false;

        private List<MultiLine> roads = new List<MultiLine>();

        public InterpolateFunction roadFunction;

        public bool enableRoads = false;
        public int roadGraphSize = 100;
        public float roadSlopeMultiplyer = 1;
        public float roadMaxSlope = 0.1f;
        public float roadMinHeight = 1;
        public float roadBaseWeight = 1;
        public float roadRandom = 0;
        public float roadWidth = 20;
        public int roadPoints = 10;

        private int meshReady = 0;

        public bool playerToRoads = false;

        public void MeshReady()
        {
            meshReady++;

            if (meshReady == chunkAmount * chunkAmount)
            {
                meshReady = 0;
                foreach(Chunk c in chunks)
                {
                    c.MeshUpdate();
                }
            }
        }

        private void Start()
        {
            if (enableRoads)
            {
                HeightMap h = HeightMapGeneration.GenerateHeightmap(HeightFunction, roadGraphSize, 15, seed, autoCache: autoCache, nonLinearScaling: nonLinearScaling, cacheAtDepth: cacheAtDepth);

                GridGraph G = new GridGraph();

                G.Construct<float>(h.GetSlopeMap(),
                    (a, b) => Mathf.Abs(a - b) * Mathf.Abs(a - b) * Mathf.Abs(a - b) * roadSlopeMultiplyer + roadBaseWeight + RandomGenerator.NormalValue(Mathf.Abs(a), Mathf.Abs(b)) * roadRandom,
                    (a, b) => true,//(Mathf.Abs(a - b) < roadMaxSlope && a > roadMinHeight && b > roadMinHeight),
                    false,
                    Vector2.zero,
                    chunkSize * zoom);

                List<Vector2> points = PointPlacement.GetPoints(roadGraphSize, 2, roadPoints, (seed * Mathf.PI).FractionalDigits());

                for (int i = 0; i < points.Count; i++)
                {
                    if (h[(int)points[i].x, (int)points[i].y] <= 0)
                    {
                        points.RemoveAt(i);
                        i--;
                    }
                }

                Debug.Log("Number of road endpoints: " + points.Count);

                for (int i = 0; i < points.Count; i++)
                {
                    for (int j = 0; j < points.Count; j++)
                    {
                        if (i != j && i < j)
                        {
                            //Node s = new Node();
                            //s.x = Mathf.FloorToInt(points[i].x);
                            //s.z = Mathf.FloorToInt(points[i].y);
                            //Node t = new Node();
                            //t.x = Mathf.FloorToInt(points[j].x);
                            //t.z = Mathf.FloorToInt(points[j].y);

                            if (points[i] == points[j])
                                continue;

                            Vector2[] road = ShortestPath.AStar<Vector2>(G, G.GetIndexOfNode(points[i]), G.GetIndexOfNode(points[j]), G.ManhattenDistance);

                            if (road != null)
                            {
                                MultiLine nroad = new MultiLine(chunkSize);

                                foreach (Vector2 v in road)
                                {
                                    nroad.AddVertex(v.x, v.y);
                                }

                                //nroad.GenerateVertices(50);

                                roads.Add(nroad);

                                //TODO: remove
                                i = points.Count;
                                j = points.Count;
                            }
                        }
                    }
                }

                Debug.Log("Number of roads: " + roads.Count);

                if (roads.Count > 0)
                {
                    Debug.Log("s: " + roads[0][0].v + ". t: " + roads[0][roads[0].Length - 1].v);
                }
            }
            

            Clean();
            Initialize();
        }

        public float HeightFunction(Vertex a, Vertex b, float r)
        {
            return (a.h + b.h) / 2 + r * ((a.v - b.v).magnitude * distanceFactor + Mathf.Abs(a.h - b.h) * heightDifferenceFactor);
        }
        public float GreenFunction(Vertex a, Vertex b, float r)
        {
            return (a.h + b.h) / 2 + r * ((a.v - b.v).magnitude * 2 + Mathf.Abs(a.h - b.h) * 0.3f);
        }

        private void Update()
        {
            UpdateMeshes();
        }

        public void Initialize()
        {
            if (init)
                Clean();

            climate.Initialize();

            if (playerToRoads)
            {
                viewer.transform.position = new Vector3(roads[0][roads[0].Length-1].v.x, 150, roads[0][roads[0].Length - 1].v.y);
                this.transform.position =   new Vector3(roads[0][roads[0].Length-1].v.x, 0,   roads[0][roads[0].Length-1].v.y);
            }

            chunks = new Chunk[chunkAmount * chunkAmount];
            newPositions = new Queue<Vector3>();

            centerChunk = transform.position - new Vector3(transform.position.x % chunkSize, 0, transform.position.z % chunkSize);
            updatedPlayerPosition = this.transform.position;

            parentChunk = new GameObject("Chunks");

            for (int i = 0; i < chunkAmount; i++)
            {
                for (int j = 0; j < chunkAmount; j++)
                {
                    GameObject go = new GameObject("Chunk" + (i + j * chunkAmount));
                    go.transform.parent = parentChunk.transform;
                    go.transform.position = centerChunk + new Vector3(i - chunkAmount / 2, 0, j - chunkAmount / 2) * chunkSize;
                    Chunk c = go.AddComponent<Chunk>();
                    c.Initialize(this);
                    chunks[i + j * chunkAmount] = c;
                }
            }

            CompleteUpdate(centerChunk);

            //JobManager.GetInstance().FinishJobs();
            //JobManager.GetInstance().ConsumeAll();

            init = true;
        }

        public void Clean()
        {
            DestroyImmediate(parentChunk);
            chunks = new Chunk[0];
            init = false;
        }

        public float RoadInfluence(Vector2 point)
        {
            if (roads == null || roads.Count == 0)
                return 0;

            float dist = float.PositiveInfinity;

            for (int i = 0; i < roads.Count; i++)
            {
                float ndist = roads[i].Distance(point);
                if (ndist < dist)
                {
                    dist = ndist;
                }
            }

            if (dist < roadWidth)
            {
                return roadFunction.Evaluate(dist.Remap(0, roadWidth, 0, 1).Clamp(0, 1));
            } else
            {
                return 0;
            }
        }

        public void UpdateMeshes()
        {
            if (!init)
                return;

            Vector3 newCenterChunk = transform.position - new Vector3(transform.position.x % chunkSize, 0, transform.position.z % chunkSize);
            Vector3 oldCenterChunk = centerChunk + Vector3.zero;
            //centerChunk = newCenterChunk;

            if (newCenterChunk == oldCenterChunk)
                return;

            //if (((centerChunk + new Vector3(chunkSize, 0, chunkSize) / 2) - transform.position).magnitude < 1 * chunkSize)
            //    return;

            if ((updatedPlayerPosition - transform.position).magnitude < chunkSize * 0.5f)
                return;

            centerChunk = newCenterChunk + Vector3.zero;

            updatedPlayerPosition = transform.position;

            //if (JobManager.GetInstance().Pending > 0)
            //{
            //    JobManager.GetInstance().FinishJobs();
            //    JobManager.GetInstance().ConsumeAll();
            //}

            Debug.Log("Update chunks (MapGenerator).");
            Stopwatch watch = new Stopwatch();
            watch.Start();

            if ((newCenterChunk - oldCenterChunk).magnitude > chunkSize * 1.415f)
            {
                CompleteUpdate(newCenterChunk);
            }
            else
            {
                IncrementalUpdate(newCenterChunk, oldCenterChunk);
            }

            watch.Stop();
            Debug.Log("Time taken: " + watch.ElapsedMilliseconds);
        }

        private void IncrementalUpdate(Vector3 newCenterChunk, Vector3 oldCenterChunk)
        {
            int cnt = 0;
            if (oldCenterChunk.x < newCenterChunk.x)
            {
                for (int i = 0; i < chunkAmount; i++)
                {
                    cnt++;
                    newPositions.Enqueue(newCenterChunk + new Vector3(chunkAmount / 2, 0, i - chunkAmount / 2) * chunkSize);
                }
            }
            if (oldCenterChunk.x > newCenterChunk.x)
            {
                for (int i = 0; i < chunkAmount; i++)
                {
                    cnt++;
                    newPositions.Enqueue(newCenterChunk + new Vector3(-chunkAmount / 2, 0, i - chunkAmount / 2) * chunkSize);
                }
            }
            if (oldCenterChunk.z < newCenterChunk.z)
            {
                for (int i = 0; i < chunkAmount; i++)
                {
                    cnt++;
                    newPositions.Enqueue(newCenterChunk + new Vector3(i - chunkAmount / 2, 0, chunkAmount / 2) * chunkSize);
                }
            }
            if (oldCenterChunk.z > newCenterChunk.z)
            {
                for (int i = 0; i < chunkAmount; i++)
                {
                    cnt++;
                    newPositions.Enqueue(newCenterChunk + new Vector3(i - chunkAmount / 2, 0, -chunkAmount / 2) * chunkSize);
                }
            }

            foreach (Chunk c in chunks)
            {
                if (c.waitingJobResult)
                    continue;

                float dist = (float)((int)chunkAmount / 2) * chunkSize;
                Vector3 newpos = c.transform.position;
                Vector3 testCenterChunk = centerChunk;
                if (Mathf.Abs(c.transform.position.x - newCenterChunk.x) > dist ||
                    Mathf.Abs(c.transform.position.z - newCenterChunk.z) > dist)
                {
                    //c.transform.position = newPositions.Dequeue();
                    newpos = newPositions.Dequeue();
                }
                UpdateChunk(c, newCenterChunk, newpos);
            }
        }

        private void CompleteUpdate(Vector3 newCenterChunk)
        {
            for (int i = 0; i < chunkAmount; i++)
            {
                for (int j = 0; j < chunkAmount; j++)
                {
                    newPositions.Enqueue(newCenterChunk + new Vector3(i - chunkAmount / 2, 0, j - chunkAmount / 2) * chunkSize);
                }
            }
            foreach (Chunk c in chunks)
            {
                UpdateChunk(c, newCenterChunk, newPositions.Dequeue());
            }
        }

        private void UpdateChunk(Chunk c, Vector3 newCenterChunk, Vector3 newpos)
        {
            int i = 0; int j = 0; int l = 0; int ln = 0;
            //Vector3 cpos = c.transform.position - new Vector3(c.transform.position.x % chunkSize, 0, c.transform.position.z % chunkSize);
            int dist = (int)Mathf.Max(Mathf.Abs(newpos.x - newCenterChunk.x), Mathf.Abs(newpos.z - newCenterChunk.z));
            //if (dist < depths.Count * chunkSize)
            //{
            //    j = (int)(dist / chunkSize);
            //}
            //else
            //{
            //    j = depths.Count - 1;
            //}
            if (dist < skips.Count * chunkSize)
            {
                l = (int)(dist / chunkSize);
            }
            else
            {
                l = skips.Count - 1;
            }
            if (dist + chunkSize < skips.Count * chunkSize)
            {
                ln = (int)(dist / chunkSize)+1;
            }
            else
            {
                ln = skips.Count - 1;
            }
            //Debug.Log(Mathf.FloorToInt(dist / chunkSize));
            c.UpdateChunk(skips[l], skips[ln], newpos, Mathf.FloorToInt(dist / chunkSize));
        }
    }
}