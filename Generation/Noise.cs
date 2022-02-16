//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.Generation
{
    public class Noise
    {
        public int NoiseRandom(int value)
        {
            return Random.Range(-1, 1) + value;
        }

        //public enum ParticleDopStrategy
        //{
        //    Normal,
        //    Random
        //}

        private static int[,] array1 = { { 0, 0, 0 }, { 0, 1, 0 }, { 0, 0, 0 } };









        public static Texture2D GenerateHightMap(int[,] map)
        {
            Texture2D retval = new Texture2D(map.GetLength(0), map.GetLength(1));

            int maxVal = 0;
            int minVal = int.MaxValue;

            foreach (int i in map)
            {
                if (i > maxVal)
                    maxVal = i;
                if (i < minVal)
                    minVal = i;
            }

            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    float value = ((float)map[i, j] - (float)minVal) / ((float)maxVal - (float)minVal);
                    retval.SetPixel(i, j, new Color(value, value, value));
                }
            }
            retval.Apply();
            return retval;
        }

        public static Texture2D GenerateHightMap(int[,] map, int floor, int ceil)
        {
            Texture2D retval = new Texture2D(map.GetLength(0), map.GetLength(1));

            int maxVal = ceil;
            int minVal = floor;

            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    float value = ((float)map[i, j] - (float)minVal) / ((float)maxVal - (float)minVal);
                    retval.SetPixel(i, j, new Color(value, value, value));
                }
            }
            retval.Apply();
            return retval;
        }

        public static void LoadTerrain(Texture2D texture2D, TerrainData aTerrain)
        {
            int h = aTerrain.heightmapResolution;
            int w = aTerrain.heightmapResolution;
            float[,] data = new float[h, w];

            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < w; j++)
                {
                    float v = texture2D.GetPixel(w, h).r;
                }
            }

            //using (var file = System.IO.File.OpenRead(aFileName))
            //using (var reader = new System.IO.BinaryReader(file))
            //{
            //    for (int y = 0; y < h; y++)
            //    {
            //        for (int x = 0; x < w; x++)
            //        {
            //            float v = (float)reader.ReadUInt16() / 0xFFFF;
            //            data[y, x] = v;
            //        }
            //    }
            //}
            aTerrain.SetHeights(0, 0, data);
        }
    }
}