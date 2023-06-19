using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.Generation
{
    public static class HeightMapGeneration
    {
        public static HeightMap GenerateHeightmap(int size,
            float dist, float heightDiff,
            int depth = 9, int seed = -1,
            float ah = -2, float bh = -2, float ch = -2,
            float offsetX = 0, float offsetZ = 0, float zoom = 1,
            int cacheAtDepth = -1,
            bool nonLinearScaling = false,
            bool autoCache = false)
        {
            Func<Vertex, Vertex, float, float> func = (Vertex a, Vertex b, float r) => (a.h + b.h) / 2 + r * ((a.xz - b.xz).magnitude * dist + Mathf.Abs(a.h - b.h) * heightDiff);

            return GenerateHeightmap(func, size, depth, seed, ah, bh, ch, offsetX, offsetZ, zoom, cacheAtDepth, null, 0, nonLinearScaling, autoCache);
        }

        public static HeightMap GenerateHeightmap(Func<Vertex, Vertex, float, float> heightFunction,
            int size,
            int depth = 9, int seed = -1,
            float ah = -2, float bh = -2, float ch = -2,
            float offsetX = 0, float offsetZ = 0, float zoom = 1,
            int cacheAtDepth = -1,
            bool nonLinearScaling = false,
            bool autoCache = false)
        {
            return GenerateHeightmap(heightFunction, size, depth, seed, ah, bh, ch, offsetX, offsetZ, zoom, cacheAtDepth, null, 0, nonLinearScaling, autoCache);
        }

        public static HeightMap GenerateHeightmap(Func<Vertex, Vertex, float, float> heightFunction,
            HeightMap old, int skips,
            int depth = 9, int seed = -1,
            float ah = -2, float bh = -2, float ch = -2,
            float offsetX = 0, float offsetZ = 0, float zoom = 1,
            int cacheAtDepth = -1,
            bool nonLinearScaling = false,
            bool autoCache = false)
        {
            return GenerateHeightmap(heightFunction, 0, depth, seed, ah, bh, ch, offsetX, offsetZ, zoom, cacheAtDepth, old, skips, nonLinearScaling, autoCache);
        }

        private static HeightMap GenerateHeightmap(Func<Vertex, Vertex, float, float> heightFunction, int size = 0,
            int depth = 9, int seed = -1,
            float ah = -2, float bh = -2, float ch = -2,
            float offsetX = 0, float offsetZ = 0, float zoom = 1,
            int cacheAtDepth = -1,
            HeightMap old = null, int skips = 1,
            bool nonLinearScaling = false,
            bool autoCache = false)
        {
            if (autoCache)
            {
                cacheAtDepth = depth - Mathf.FloorToInt(Mathf.Log(zoom * zoom / 0.5f, 4));
                if (cacheAtDepth < 1)
                    cacheAtDepth = -1;
            }

            float s = 1 / (float)seed;

            EquilateralTriangle generator = new EquilateralTriangle(cacheAtDepth);

            if (ah < -1 || ah > 1)
                ah = Noise.Value(s, 0.5236f);
            if (bh < -1 || bh > 1)
                bh = Noise.Value(ah, -0.3251f);
            if (ch < -1 || ch > 1)
                ch = Noise.Value(ah, bh);

            // Height of TRIANGLE
            float h = Mathf.Sqrt(3) / 2;
            Vertex a = new Vertex(new Vector2(0, 0), 0, ch);
            Vertex b = new Vertex(new Vector2(0.5f, h), 0, ah);
            Vertex c = new Vertex(new Vector2(1, 0), 0, bh);

            offsetX = offsetX;
            offsetZ = offsetZ;

            float[,] m = new float[size, size];

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    generator.infcounter = 0;

                    Vector2 p = new Vector2(offsetX + (float)i / (float)(size - 1) / zoom,
                        offsetZ + (float)j / (float)(size - 1) / zoom);

                    if (old != null && i % (skips) == 0 && j % (skips) == 0)
                    {
                        m[i, j] = old[i / skips, j / skips];
                    }
                    else
                    {
                        //m[i, j] = generator.HeightAtPoint(p, a, b, c, depth, heightFunction, cacheAtDepth);

                        if (nonLinearScaling)
                        {
                            float v = m[i, j];

                            m[i, j] = v * v * v;
                        }
                    }
                }
            }

            return new HeightMap(m);
        }
    }
}

/*else
                    {
                        bool useCached = false;
                        foreach (HMapTriangle cached in cachedVertices)
                        {
                            if (actualDepth < cacheAtDepth)
                                break;
                            if (PointInTriangle(p, cached.a.v, cached.b.v, cached.c.v))
                            {
                                useCached = true;
                                m[i, j] = HeightAtPoint(p, cached.a, cached.b, cached.c,
                                    actualDepth - (actualDepth - actualCacheAtDepth), heightFunction);
                            }
                        }

                        if (!useCached)
                            m[i, j] = HeightAtPoint(p, a, b, c, actualDepth, heightFunction);

                        if (nonLinearScaling)
                        {
                            float v = m[i, j];

                            m[i, j] = v * v * v;
                        }
                    }
*/