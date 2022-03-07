using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.Generation
{
    public class ProceduralGeneration
    {
        public static HeightMap DiamonSquare(int n, float var, int seed = -1)
        {
            int size = (int)Mathf.Pow(2, n) + 1;

            float[,] h = new float[size, size];

            h[0, 0] = 0.153242f;
            h[0, size - 1] = -0.231f;
            h[size - 1, 0] = 0.56473f;
            h[size - 1, size - 1] = -0.562674f;

            if (seed > 0)
            {
                UnityEngine.Random.InitState(seed);
            }

            Diamond(h, (0, size - 1), (0, size - 1), var);

            return new HeightMap(h);
        }

        private static void Square(float[,] heightmap, (int start, int end) X, (int start, int end) Y, float heightDiff)
        {
            int size = X.end - X.start;
            int midX = X.start + size / 2;
            int midY = Y.start + size / 2;

            float p1 = 0, p2 = 0, p3 = 0, p4 = 0, pn = 0;
            if (X.start >= 0)
            {
                p1 = heightmap[X.start, midY];
                pn++;
            }
            if (X.end < heightmap.GetLength(0))
            {
                p2 = heightmap[X.end, midY];
                pn++;
            }
            if (Y.start >= 0)
            {
                p3 = heightmap[midX, Y.start];
                pn++;
            }
            if (Y.end < heightmap.GetLength(0))
            {
                p4 = heightmap[midX, Y.end];
                pn++;
            }

            heightmap[midX, midY] =
                (p1 + p2 + p3 + p4) / pn + (UnityEngine.Random.value * 2 - 1) * heightDiff * ((float)size / (float)heightmap.GetLength(0));

            if (size > 1)
            {
                if (X.start >= 0 && Y.start >= 0)
                    Diamond(heightmap, (X.start, midX), (Y.start, midY), heightDiff);
                if (X.start >= 0 && Y.end < heightmap.GetLength(0))
                    Diamond(heightmap, (X.start, midX), (midY, Y.end), heightDiff);
                if (X.end < heightmap.GetLength(0) && Y.start >= 0)
                    Diamond(heightmap, (midX, X.end), (Y.start, midY), heightDiff);
                if (X.end < heightmap.GetLength(0) && Y.end < heightmap.GetLength(0))
                    Diamond(heightmap, (midX, X.end), (midY, Y.end), heightDiff);
            }

        }
        private static void Diamond(float[,] heightmap, (int start, int end) X, (int start, int end) Y, float heightDiff)
        {
            int size = X.end - X.start;
            int midX = X.start + size / 2;
            int midY = Y.start + size / 2;

            heightmap[midX, midY] =
                (heightmap[X.start, Y.start] + heightmap[X.end, Y.start] +
                heightmap[X.start, Y.end] + heightmap[X.end, Y.end]) / 4 + (UnityEngine.Random.value * 2 - 1) * heightDiff * ((float)size / (float)heightmap.GetLength(0)) * 1.41f;

            if (size > 1)
            {
                Square(heightmap, (X.start - size / 2, midX), (Y.start, Y.end), heightDiff);
                Square(heightmap, (midX, X.end + size / 2), (Y.start, Y.end), heightDiff);
                Square(heightmap, (X.start, X.end), (Y.start - size / 2, midY), heightDiff);
                Square(heightmap, (X.start, X.end), (midY, Y.end + size / 2), heightDiff);
            }
        }





    }
}