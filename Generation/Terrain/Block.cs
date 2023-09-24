using JStuff.Generation;
using JStuff.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.Generation.Terrain
{
    public class Block : MonoBehaviour, IConsumer
    {
        [Header("Debug")]
        public bool initialized = false;

        bool inJob = false;

        public WorldTerrain terrain;

        TerrainGraph graph;
        public MeshFilter meshFilter;
        public MeshFilter colliderMeshFilter;
        public MeshCollider meshCollider;
        Material material;

        BlockData oldData;
        public BlockData currentData;

        public FoliageInstancing foliageInstancing;

        // For calculating seam normals
        public bool newNormals = true;
        //public Vector3[] normals;
        //public Color[] colormap;
        public int[] seamSize = new int[] { -1, -1, -1, -1 };
        public bool[] seamExtrusion = new bool[] { false, false, false, false };

        public bool seamless = false;

        public GameObject colliderObject;

        public Vector3 centerPositionOfTarget;
        public Vector3 targetPosition;
        public Vector3 oldTargetPosition;

        public bool waitingJobResult = false;
        public bool waitingCoroutine = false;
        public string coroutine = "";

        public List<GameObject> terrainGameObjects = new List<GameObject>();

        public int iteration = 0;
        int terrainObjectsAmount = 0;

        public int currentMeshLOD = -1;
        public int currentTerrainObjectLOD = -1;
        public int targetMeshLOD = -1;
        public int targetTerrainObjectLOD = -1;
        public bool targetHasCollider = false;
        public bool currentHasCollider = false;

        public int Priority => priority;
        int priority = 0;

        public static TerrainCoordinate[] DirectionCoordinate = new TerrainCoordinate[]
        {
            new TerrainCoordinate(1, 0),
            new TerrainCoordinate(0, 1),
            new TerrainCoordinate(-1, 0),
            new TerrainCoordinate(0, -1),
        };

        public bool RedoSeamExtrusions(int newMeshLength)
        {
            for (int i = 0; i < 4; i++)
            {
                if (terrain.blockOfCoordinates.ContainsKey(GetCoordinates() + DirectionCoordinate[i]))
                {
                    int otherLOD = GetNeighborLOD(i);

                    if (otherLOD <= targetMeshLOD && seamExtrusion[i])
                    {
                        // There should be no seam extrusion, but there is
                        return true;
                    }
                    else if (otherLOD > targetMeshLOD && !seamExtrusion[i])
                    {
                        // There should be seam extrusion, but there isn't
                        return true;
                    }
                }
            }

            return false;
        }

        public bool RedoNormals(int newMeshLength)
        {
            for (int i = 0; i < 4; i++)
            {
                if (terrain.blockOfCoordinates.ContainsKey(GetCoordinates() + DirectionCoordinate[i]))
                {
                    if (seamSize[i] * seamSize[i] != newMeshLength)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void Start()
        {
            meshFilter = GetComponent<MeshFilter>();
        }

        public void Initialize(WorldTerrain worldTerrain, TerrainGraph graph, Material material)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
            gameObject.AddComponent<MeshRenderer>().material = material;
            this.material = material;


            colliderObject = new GameObject("Collider");
            colliderObject.transform.parent = this.transform;
            colliderObject.transform.localPosition = Vector3.zero;
            meshCollider = colliderObject.AddComponent<MeshCollider>();
            colliderObject.SetActive(false);
            colliderObject.layer = gameObject.layer;


            this.graph = graph;
            //this.graph.GetConnections(graph.GetPorts());
            //graph.InitializeGraph();

            terrain = worldTerrain;

            initialized = true;

            oldTargetPosition = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            foliageInstancing = gameObject.AddComponent<FoliageInstancing>();
            foliageInstancing.SetSettings(terrain.foliageLODSettings, worldTerrain.blockSize, worldTerrain.cameraTransform);

            //meshLOD = new Mesh[worldTerrain.meshLOD.Length];
        }

#if UNITY_EDITOR
        public void EditorGenerateRenderMesh()
        {
            BlockData data = ((TerrainGraph)graph).EvaluateGraph(TerrainRoot.BlockDataType.RenderMesh);
            data._meshRendererData = data.meshRendererData;
            data._colormap = data.colormap;
            ConsumeJob(data);
        }

        public void EditorGenerateAll()
        {
            BlockData data = ((TerrainGraph)graph).EvaluateGraph();
            ConsumeJob(data);
        }
#endif

        public void UpdateBlock(Vector3 centerPosition, Vector3 newPosition)
        {
            //(int LOD, int index) = terrain.GetTerrainLOD(Vector3.Distance(newPosition, centerPosition));

            float distance = Vector3.Distance(centerPosition, newPosition);

            int meshLOD = terrain.LODSettings.GetLOD(distance, TerrainLODSettings.LODTypes.Mesh);
            int terrainObjectLOD = terrain.LODSettings.GetLOD(distance, TerrainLODSettings.LODTypes.TerrainObjects);

            if (waitingJobResult)
            {
                Debug.Log("Waiting job result!");
                JobManagerComponent.instance.manager.FinishJobs();
            }

            //graph.LOD = LOD;
            //graph.CenterPosition = new Vector2(centerPosition.x, centerPosition.z);
            //graph.ChunkPosition = new Vector2(newPosition.x, newPosition.z);

            graph.SetChunk(new Vector2(newPosition.x, newPosition.z), new Vector2(centerPosition.x, centerPosition.z), meshLOD, terrainObjectLOD);

            targetPosition = newPosition;
            //priority = index;
            targetMeshLOD = meshLOD;
            targetTerrainObjectLOD = terrainObjectLOD;
            centerPositionOfTarget = centerPosition;
            targetHasCollider = Vector3.Distance(terrain.GetCenterChunkPosition(), targetPosition) < terrain.colliderAtDistance;

            if (iteration != 0 && (meshLOD == currentMeshLOD && newPosition == oldTargetPosition) && currentTerrainObjectLOD == terrainObjectLOD)
            {
                int newSeamLength = ((currentData.meshRendererData.sizeX - 1) / targetMeshLOD) + 1;
                int newMeshLength = newSeamLength * newSeamLength;
                if (RedoSeamExtrusions(newMeshLength) || RedoNormals(newMeshLength))
                {
                    TerrainPool.QueueRenderMesh(this, iteration, currentData._meshRendererData, currentData._colormap, targetPosition, RedoSeamExtrusions(newMeshLength), RedoNormals(newMeshLength), false);
                }
                return;
            }

            if (terrain.enableThreading)
            {
                waitingJobResult = true;
                JobManagerComponent.instance.manager.AddJob(this, Job, graph);
            }
            else
            {
                waitingJobResult = true;
                currentData = Job(graph) as BlockData;
                ConsumeJob(currentData);
            }
        }

        public object Job(object graph)
        {
            TerrainRoot.BlockDataType jobFlags = TerrainRoot.BlockDataType.None;

            if (targetPosition == oldTargetPosition)
            {
                if (targetTerrainObjectLOD != currentTerrainObjectLOD && targetTerrainObjectLOD > -1)
                    jobFlags = jobFlags | TerrainRoot.BlockDataType.TerrainObjects;

                if (currentMeshLOD < 0 && targetMeshLOD > -1)
                {
                    jobFlags = jobFlags | TerrainRoot.BlockDataType.RenderMesh;
                }

                if (targetHasCollider && !currentHasCollider)
                    jobFlags = jobFlags | TerrainRoot.BlockDataType.ColliderMesh;

            } else
            {
                if (targetMeshLOD > -1)
                {
                    jobFlags = jobFlags | TerrainRoot.BlockDataType.RenderMesh;
                }

                if (targetTerrainObjectLOD > -1)
                    jobFlags = jobFlags | TerrainRoot.BlockDataType.TerrainObjects;

                if (targetHasCollider)
                    jobFlags = jobFlags | TerrainRoot.BlockDataType.ColliderMesh;
            }

            BlockData retval = ((TerrainGraph)graph).EvaluateGraph(jobFlags);

            if (targetPosition == oldTargetPosition)
            {
                // Reuse
                if (!jobFlags.HasFlag(TerrainRoot.BlockDataType.TerrainObjects))
                {
                    retval.foliage = currentData.foliage;
                    retval.terrainObjects = currentData.terrainObjects;
                }

                if (!jobFlags.HasFlag(TerrainRoot.BlockDataType.RenderMesh))
                {
                    retval.meshRendererData = currentData.meshRendererData;
                    retval.colormap = currentData.colormap;
                }

                if (!jobFlags.HasFlag(TerrainRoot.BlockDataType.ColliderMesh))
                    retval.meshColliderData = currentData.meshColliderData;
            }

            //TODO: Make color of mesh corners correct. They are incorrect right now!

            if (targetMeshLOD > -1)
            {
                // Set changable data
                retval._meshRendererData = TerrainMeshGeneration.GenerateLODMeshData(retval.meshRendererData, targetMeshLOD);
                retval._colormap = TerrainMeshGeneration.GenerateLODColormap(retval.colormap, targetMeshLOD);

                retval._normals = new Vector3[retval._meshRendererData.vertices.Length];
                retval.seamExtrusion = new bool[] { false, false, false, false };
                retval.seamSize = new int[] { -1, -1, -1, -1 };

                retval._meshRendererData.CalculateUnormalizedNormals(ref retval._normals);

                foreach (var direction in GetLODEdgeDirections())
                {
                    int LOD = GetNeighborLOD(direction);

                    int LODDiff = LOD / targetMeshLOD;

                    if (LODDiff == 0)
                        continue;

                    retval.seamExtrusion[direction] = true;
                    Seam.ExtrudeEdgeVertices(retval._meshRendererData, LODDiff, direction);
                }
            }

            if (targetHasCollider)
            {
                retval._meshColliderData = TerrainMeshGeneration.GenerateLODMeshData(retval.meshColliderData, terrain.colliderLOD);
            }

            return retval;
        }

        public void ConsumeJob(object data)
        {
            currentData = (BlockData)data;
            waitingJobResult = false;
            ApplyData();
        }

        public void JobFailed()
        {
            throw new System.NotImplementedException("Job failed!");
        }

        public void UpdateValues(Vector3 centerPosition, Vector3 newPosition)
        {
            graph.CenterPosition = new Vector2(centerPosition.x, centerPosition.z);
            graph.ChunkPosition = new Vector2(newPosition.x, newPosition.z);
            targetPosition = newPosition;
        }

        public void ApplyData()
        {
            //TODO: move most of this into TerrainPool and just send the job data there - less copying of data
            bool newPos = false;
            seamExtrusion = currentData.seamExtrusion;
            seamSize = currentData.seamSize;

            if (targetPosition != oldTargetPosition)
            {
                iteration++;
                newPos = true;
                seamless = false;
            }

            // Remove terrain objects
            if (newPos)
            {
                for (int i = 0; i < terrainGameObjects.Count; i++)
                {
                    TerrainPool.QueueDestroyTerrainObject(terrainGameObjects[i], this, iteration);
                    //TerrainPool.DestroyTerrainObject(terrainGameObjects[i]);
                    //terrainGameObjects.RemoveAt(i);
                    //i--;
                }
                //terrainGameObjects.Clear();
            }
            else if (currentData.terrainObjects != null && currentData.terrainObjects.Count != terrainObjectsAmount)
            {
                int size = terrainGameObjects.Count;
                int j = (currentData.terrainObjects.Count > 0) ? currentData.terrainObjects.Count : 0;
                for (int i = j; i < terrainGameObjects.Count; i++)
                {
                    TerrainPool.QueueDestroyTerrainObject(terrainGameObjects[i], this, iteration);
                    //TerrainPool.DestroyTerrainObject(terrainGameObjects[i]);
                    //terrainGameObjects.RemoveAt(i);
                    //i--;
                }
            }

            // Mesh renderer
            //(int LOD, int index) = terrain.GetTerrainLOD(Vector3.Distance(terrain.GetCenterChunkPosition(), targetPosition));

            if (newPos)
            {
                SetPosition(targetPosition);
            }

            if (targetMeshLOD != currentMeshLOD || newPos)
            {
                //meshFilter.sharedMesh = TerrainMeshGeneration.GenerateMesh(currentData.meshLOD[index], currentData.colormapLOD[index]);
                TerrainPool.QueueRenderMesh(this, iteration, currentData._meshRendererData, currentData._colormap, targetPosition, false, false, true);
                currentMeshLOD = targetMeshLOD;
            } else if (RedoSeamExtrusions(currentData._meshRendererData.vertices.Length) || RedoNormals(currentData._meshRendererData.vertices.Length))
            {
                TerrainPool.QueueRenderMesh(this, iteration, currentData._meshRendererData, currentData._colormap, targetPosition, 
                    RedoSeamExtrusions(currentData._meshRendererData.vertices.Length), 
                    RedoNormals(currentData._meshRendererData.vertices.Length),
                    false);
            }

            // Mesh collider
            if (oldData == null || newPos || targetHasCollider != currentHasCollider)
            {
                if (currentData.meshColliderData != null && targetHasCollider)
                {
                    TerrainPool.QueueColliderMesh(
                        this, iteration, currentData.meshColliderData);
                }
                else if (colliderObject.activeInHierarchy)
                {
                    colliderObject.SetActive(false);
                }
            }

            // Spawn terrain objects
            if (currentData.terrainObjects != null)
            {
                if (currentData.terrainObjects.Count > terrainObjectsAmount)
                {
                    int j = (currentData.terrainObjects.Count > 0) ? terrainObjectsAmount : 0;
                    for (int i = j; i < currentData.terrainObjects.Count; i++)
                    {
                        TerrainPool.QueueTerrainObject(currentData.terrainObjects[i], this, iteration);
                        //terrainGameObjects.Add(TerrainPool.CreateTerrainObject(currentData.terrainObjects[i], this));
                    }

                    terrainObjectsAmount = currentData.terrainObjects.Count;
                }
            }

            if (currentTerrainObjectLOD != targetTerrainObjectLOD || (newPos && currentTerrainObjectLOD > -1))
            {
                TerrainPool.QueueFoliage(currentData.foliage, this, iteration);
            }

            if (terrain.blockOfCoordinates.ContainsKey(new TerrainCoordinate(terrain.blockSize, oldTargetPosition)) &&
                terrain.blockOfCoordinates[new TerrainCoordinate(terrain.blockSize, oldTargetPosition)] == this)
            {
                terrain.blockOfCoordinates.Remove(new TerrainCoordinate(terrain.blockSize, oldTargetPosition));
            }

            if (terrain.blockOfCoordinates.ContainsKey(GetCoordinates()))
            {
                terrain.blockOfCoordinates[GetCoordinates()] = this;
            }
            else
            {
                terrain.blockOfCoordinates.Add(GetCoordinates(), this);
            }

            currentTerrainObjectLOD = targetTerrainObjectLOD;
            currentHasCollider = targetHasCollider;
            oldTargetPosition = targetPosition;
            oldData = currentData;
        }

        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        public TerrainCoordinate GetCoordinates()
        {
            return new TerrainCoordinate(terrain.blockSize, targetPosition);
        }

        public static Vector2 CenterDirection(TerrainCoordinate coordinates, TerrainCoordinate centerCoordinates)
        {
            Vector2 retval = new Vector2();

            if (centerCoordinates.x > coordinates.x)
                retval.x = 1;
            if (centerCoordinates.y > coordinates.y)
                retval.y = 1;
            if (centerCoordinates.x < coordinates.x)
                retval.x = -1;
            if (centerCoordinates.y < coordinates.y)
                retval.y = -1;

            return retval;
        }

        public static List<int> AwayCenterDirections(TerrainCoordinate coordinates, TerrainCoordinate centerCoordinates)
        {
            List<int> retval = new List<int>();

            if (centerCoordinates.x < coordinates.x)
                retval.Add(0);
            if (centerCoordinates.y < coordinates.y)
                retval.Add(1);
            if (centerCoordinates.x > coordinates.x)
                retval.Add(2);
            if (centerCoordinates.y > coordinates.y)
                retval.Add(3);

            return retval;
        }

        public static List<int> AwayCenterDirections(Vector3 blockPosition, Vector3 centerPosition)
        {
            List<int> retval = new List<int>();

            if (centerPosition.x < blockPosition.x)
                retval.Add(0);
            if (centerPosition.z < blockPosition.z)
                retval.Add(1);
            if (centerPosition.x > blockPosition.x)
                retval.Add(2);
            if (centerPosition.z > blockPosition.z)
                retval.Add(3);

            return retval;
        }

        public bool IsLODEdge(int direction)
        {
            TerrainCoordinate otherCoordinate = TerrainCoordinate.DirectionCoordinate[direction] + GetCoordinates();

            float distance = Vector3.Distance(otherCoordinate * terrain.blockSize, centerPositionOfTarget);
            int otherLOD = terrain.LODSettings.GetLOD(distance, TerrainLODSettings.LODTypes.Mesh);

            //if (!terrain.blockOfCoordinates.ContainsKey(otherCoordinate))
            //{
            //    return false;
            //}

            //int otherLOD = terrain.blockOfCoordinates[otherCoordinate].targetLOD;

            return otherLOD > targetMeshLOD;
        }

        public int GetNeighborLOD(int direction)
        {
            TerrainCoordinate otherCoordinate = TerrainCoordinate.DirectionCoordinate[direction] + GetCoordinates();

            float distance = Vector3.Distance(otherCoordinate * terrain.blockSize, centerPositionOfTarget);
            int LOD = terrain.LODSettings.GetLOD(distance, TerrainLODSettings.LODTypes.Mesh);
            //(int LOD, int index) = terrain.GetTerrainLOD(distance);

            return LOD;

            //TerrainCoordinate otherCoordinate = TerrainCoordinate.DirectionCoordinate[direction] + job.block.GetCoordinates();

            //if (!job.block.terrain.blockOfCoordinates.ContainsKey(otherCoordinate))
            //    continue;

            //int LOD = job.block.terrain.blockOfCoordinates[otherCoordinate].targetLOD;
        }

        public List<int> GetLODEdgeDirections()
        {
            List<int> retval = new List<int>();

            TerrainCoordinate blockCoordinates = GetCoordinates();
            TerrainCoordinate centerCoordinates = new TerrainCoordinate(terrain.blockSize, centerPositionOfTarget);

            foreach (var direction in AwayCenterDirections(targetPosition, centerPositionOfTarget))
            {
                if (IsLODEdge(direction))
                    retval.Add(direction);
            }

            return retval;
        }

        //public float HeightAtPoint(float x, float z)
        //{
        //    int ax = Mathf.FloorToInt(((x - currentPosition.x) / 
        //        mother.chunkSize * currentHeightMap.Width));
        //    int az = Mathf.FloorToInt(((z - currentPosition.z) /
        //        mother.chunkSize * currentHeightMap.Width));

        //    float h = 0;
        //    int i = 0;

        //    try
        //    {
        //        h = currentHeightMap[ax, az];
        //    }
        //    catch
        //    {
        //        throw new System.Exception("Cannot get height from point. x: " + ax + ". z: " + az);
        //    }

        //    return h * mother.meshHeightMultiplier * mother.heightmapMultiplier;
        //}
    }
}