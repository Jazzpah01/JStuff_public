using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.Generation.Terrain
{
    public class WorldTerrain : MonoBehaviour
    {
        public TerrainGraph graph;
        [Min(1)] public int terrainHalfsize;
        public Vector2 offset;

        public Material material;
        public Block[] blocks;

        Queue<Vector3> newPositions;

        Vector3 centerChunk;
        public Vector3 updatedPlayerPosition;

        GameObject parentChunk;

        public float chunkSize;
        public float scale;
        public int seed;
        public float zoom;

        public bool enableThreading = false;

        public List<GameObject> terrainObjectPrefabs;

        private void Start()
        {
            Populate();
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
                    go.transform.position = centerChunk + new Vector3(i - chunkAmount / 2, 0, j - chunkAmount / 2) * chunkSize;
                    Block c = go.AddComponent<Block>();
                    c.Initialize(this, (TerrainGraph)graph.Clone(), material);
                    blocks[i + j * chunkAmount] = c;
                }
            }

            UpdateAll(centerChunk);
        }

        public void UpdateAll(Vector3 newCenterChunk)
        {
            for (int i = 0; i < Mathf.Sqrt(blocks.Length); i++)
            {
                for (int j = 0; j < Mathf.Sqrt(blocks.Length); j++)
                {
                    newPositions.Enqueue(newCenterChunk + new Vector3(i - Mathf.Sqrt(blocks.Length) / 2, 0, j - Mathf.Sqrt(blocks.Length) / 2) * chunkSize);
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

        private void Update()
        {
            //if ((centerChunk + new Vector3(chunkSize/2, 0, chunkSize / 2) - transform.position).magnitude > chunkSize)
            Vector3 newCenterChunk = transform.position - new Vector3(transform.position.x % chunkSize, 0, transform.position.z % chunkSize);

            if (centerChunk != newCenterChunk)
            {
                UpdateAll(newCenterChunk);
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
    }
}