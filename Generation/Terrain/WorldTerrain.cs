using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.Threading;

namespace JStuff.Generation.Terrain
{
    public class WorldTerrain : MonoBehaviour
    {
        public TerrainGraph graph;
        [Min(1)] public int terrainHalfsize;
        public Vector2 offset;

        public Material material;
        public Block[] blocks;

        public Transform cameraTransform;

        Queue<Vector3> newPositions;

        Vector3 centerChunk;
        public Vector3 updatedPlayerPosition;

        GameObject parentChunk;

        public bool raymarchingTerrain = false;

        private float chunkSize;
        private float scale;
        private int seed;
        private float zoom;

        public bool enableThreading = false;

        public List<GameObject> terrainObjectPrefabs;

        private void Start()
        {
            

            transform.position = new Vector3(cameraTransform.position.x, 0, cameraTransform.position.z);

            chunkSize = graph.chunkSize;
            scale = graph.scale;
            seed = graph.seed;
            zoom = graph.zoom;

            if (raymarchingTerrain)
            {
                material.SetFloat("Scale", scale);
                material.SetFloat("Stretch", chunkSize * zoom);
            }

            Populate();
            JobManagerComponent.instance.manager.FinishJobs();
        }

        bool shouldUpdate = false;

        private void Update()
        {
            if ((new Vector3(cameraTransform.position.x, 0, cameraTransform.position.z) - transform.position).magnitude > chunkSize / 2f && !shouldUpdate)
            {
                transform.position = new Vector3(cameraTransform.position.x, 0, cameraTransform.position.z);
                shouldUpdate = true;
            }

            //if ((centerChunk + new Vector3(chunkSize/2, 0, chunkSize / 2) - transform.position).magnitude > chunkSize)
            Vector3 newCenterChunk = transform.position - new Vector3(transform.position.x % chunkSize, 0, transform.position.z % chunkSize);

            if (shouldUpdate)
            {
                if (JobManagerComponent.instance.manager.Pending != 0)
                {
                    JobManagerComponent.instance.manager.FinishJobs();
                }

                DynamicUpdate(newCenterChunk, centerChunk);
                centerChunk = newCenterChunk;
                shouldUpdate = false;
            }

        }

        public void Populate()
        {
            int chunkAmount = terrainHalfsize * 2 + 1;
            blocks = new Block[chunkAmount * chunkAmount];
            newPositions = new Queue<Vector3>();

            centerChunk = transform.position - new Vector3(transform.position.x % chunkSize, 0, transform.position.z % chunkSize);
            updatedPlayerPosition = transform.position;

            parentChunk = new GameObject("Chunks");

            for (int i = 0; i < chunkAmount; i++)
            {
                for (int j = 0; j < chunkAmount; j++)
                {
                    GameObject go = new GameObject("Chunk" + (i + j * chunkAmount));
                    go.transform.parent = parentChunk.transform;
                    go.transform.position = centerChunk + new Vector3(i - terrainHalfsize, 0, j - terrainHalfsize) * chunkSize;
                    //go.transform.position = new Vector3(30 * chunkSize, 0, 30 * chunkSize);
                    Block c = go.AddComponent<Block>();
                    c.Initialize(this, (TerrainGraph)graph.Clone(), material);
                    blocks[i + j * chunkAmount] = c;
                }
            }

            UpdateAll(centerChunk);
            //DynamicUpdate(centerChunk, new Vector3(30 * chunkSize, 0, 30 * chunkSize));
        }

        public void UpdateAll(Vector3 newCenterChunk)
        {
            //for (int i = 0; i < terrainHalfsize * 2 + 1; i++)
            //{
            //    for (int j = 0; j < terrainHalfsize * 2 + 1; j++)
            //    {
            //        //newPositions.Enqueue(newCenterChunk + new Vector3(i - Mathf.Sqrt(blocks.Length) / 2, 0, j - Mathf.Sqrt(blocks.Length) / 2) * chunkSize);
            //        newPositions.Enqueue(newCenterChunk + new Vector3(i - terrainHalfsize, 0, j - terrainHalfsize) * chunkSize);
            //    }
            //}
            for (int i = -terrainHalfsize; i < terrainHalfsize + 1; i++)
            {
                for (int j = -terrainHalfsize; j < terrainHalfsize + 1; j++)
                {
                    //newPositions.Enqueue(newCenterChunk + new Vector3(i - Mathf.Sqrt(blocks.Length) / 2, 0, j - Mathf.Sqrt(blocks.Length) / 2) * chunkSize);
                    newPositions.Enqueue(newCenterChunk + new Vector3(i, 0, j) * chunkSize);
                }
            }
            foreach (Block c in blocks)
            {
                UpdateBlock(c, newCenterChunk, newPositions.Dequeue());
            }

            centerChunk = newCenterChunk;
        }

        public void UpdateBlock(Block b, Vector3 newCenterChunk, Vector3 newpos)
        {
            b.UpdateBlock(newCenterChunk, newpos);
        }

        private void DynamicUpdate(Vector3 newCenterChunk, Vector3 oldCenterChunk)
        {
            //int x_diff = Mathf.RoundToInt(Mathf.Abs((oldCenterChunk - newCenterChunk).x)/chunkSize);
            //int y_diff = Mathf.RoundToInt(Mathf.Abs((oldCenterChunk - newCenterChunk).y)/chunkSize);



            List<Block> blocksToUpdate = new List<Block>();
            List<Block> blocksToKeep = new List<Block>();

            for (int i = -terrainHalfsize; i < terrainHalfsize + 1; i++)
            {
                for (int j = -terrainHalfsize; j < terrainHalfsize + 1; j++)
                {
                    Vector3 diffPosition = newCenterChunk + new Vector3(i * chunkSize, 0, j * chunkSize) - oldCenterChunk;

                    (int x, int y) diffPos = ChunkPosition(diffPosition);

                    if (Mathf.Abs(diffPos.x) > terrainHalfsize || Mathf.Abs(diffPos.y) > terrainHalfsize)
                    {
                        newPositions.Enqueue(newCenterChunk + new Vector3(i * chunkSize, 0, j * chunkSize));
                    }
                }
            }

            (int x, int y) newPos = ChunkPosition(newCenterChunk);

            foreach (Block b in blocks)
            {
                (int x, int y) blockPos = ChunkPosition(b.targetPosition);

                (int x, int y) diffPos = ChunkPosition(new Vector2(Mathf.Abs(b.targetPosition.x - newCenterChunk.x), Mathf.Abs(b.targetPosition.z - newCenterChunk.z)));

                if (diffPos.x > terrainHalfsize || diffPos.y > terrainHalfsize)
                {
                    b.UpdateBlock(newCenterChunk, newPositions.Dequeue());
                } else
                {
                    b.UpdateBlock(newCenterChunk, b.targetPosition);
                }
            }

            
        }

        private void IncrementalUpdate(Vector3 newCenterChunk, Vector3 oldCenterChunk)
        {
            int terrainHalfsize = this.terrainHalfsize * 2 + 1;
            //int terrainHalfsize = this.terrainHalfsize + 1;

            int cnt = 0;
            if (oldCenterChunk.x < newCenterChunk.x)
            {
                for (int i = 0; i < terrainHalfsize; i++)
                {
                    cnt++;
                    newPositions.Enqueue(newCenterChunk + new Vector3(terrainHalfsize / 2, 0, i - terrainHalfsize / 2) * chunkSize);
                }
            }
            if (oldCenterChunk.x > newCenterChunk.x)
            {
                for (int i = 0; i < terrainHalfsize; i++)
                {
                    cnt++;
                    newPositions.Enqueue(newCenterChunk + new Vector3(-terrainHalfsize / 2, 0, i - terrainHalfsize / 2) * chunkSize);
                }
            }
            if (oldCenterChunk.z < newCenterChunk.z)
            {
                for (int i = 0; i < terrainHalfsize; i++)
                {
                    cnt++;
                    newPositions.Enqueue(newCenterChunk + new Vector3(i - terrainHalfsize / 2, 0, terrainHalfsize / 2) * chunkSize);
                }
            }
            if (oldCenterChunk.z > newCenterChunk.z)
            {
                for (int i = 0; i < terrainHalfsize; i++)
                {
                    cnt++;
                    newPositions.Enqueue(newCenterChunk + new Vector3(i - terrainHalfsize / 2, 0, -terrainHalfsize / 2) * chunkSize);
                }
            }

            foreach (Block c in blocks)
            {
                if (c.waitingJobResult)
                    continue;

                float dist = terrainHalfsize / 2 * chunkSize;
                Vector3 newpos = c.transform.position;
                Vector3 testCenterChunk = centerChunk;
                if (Mathf.Abs(c.transform.position.x - newCenterChunk.x) > dist ||
                    Mathf.Abs(c.transform.position.z - newCenterChunk.z) > dist)
                {
                    //c.transform.position = newPositions.Dequeue();
                    newpos = newPositions.Dequeue();
                }
                UpdateBlock(c, newCenterChunk, newpos);
            }

            centerChunk = newCenterChunk;
        }

        private (int x, int y) ChunkPosition(Vector2 pos)
        {
            int x_ = Mathf.RoundToInt(pos.x / chunkSize);
            int y_ = Mathf.RoundToInt(pos.y / chunkSize);

            return (x_, y_);
        }

        private (int x, int y) ChunkPosition(Vector3 pos)
        {
            int x_ = Mathf.RoundToInt(pos.x / chunkSize);
            int y_ = Mathf.RoundToInt(pos.z / chunkSize);

            return (x_, y_);
        }

        private Vector2 ChunkPosition((int x, int y) pos)
        {
            return new Vector2(pos.x * chunkSize, pos.y * chunkSize);
        }
    }
}