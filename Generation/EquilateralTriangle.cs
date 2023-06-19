using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace JStuff.Generation
{
    public class EquilateralTriangle
    {
        public List<Triangle> cachedVertices = new List<Triangle>();
        public HashSet<Triangle> cachedVerticesHS = new HashSet<Triangle>();
        int cacheAtDepth;

        public int infcounter = 0;
        public int iterations = 0;
        public int usedCaching = 0;
        public bool cacheError = false;

        public int Triangles => cachedVertices.Count;

        public EquilateralTriangle(int cacheAtDepth = -1)
        {
            this.cacheAtDepth = cacheAtDepth;
        }

        public HeightMap GetHeightMap(int size = 129,
            int depth = 9, float h = 0.35f, float d = 0.55f, int seed = 42, 
            float ah = -2, float bh = -2, float ch = -2,
            float offsetX = 0, float offsetZ = 0, float zoom = 1)
        {
            cacheAtDepth = depth - Mathf.FloorToInt(Mathf.Log(zoom * zoom / 0.5f, 4));
            if (cacheAtDepth < 1)
                cacheAtDepth = -1;

            float s = 1 / (float)seed;

            EquilateralTriangle generator = new EquilateralTriangle(cacheAtDepth);

            if (ah < -1 || ah > 1)
                ah = Noise.Value(s, 0.5236f);
            if (bh < -1 || bh > 1)
                bh = Noise.Value(ah, -0.3251f);
            if (ch < -1 || ch > 1)
                ch = Noise.Value(ah, bh);

            // Height of TRIANGLE
            float hei = Mathf.Sqrt(3) / 2;
            Vertex a = new Vertex(new Vector2(0, 0), 0, ch);
            Vertex b = new Vertex(new Vector2(0.5f, hei), 0, ah);
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

                    m[i, j] = generator.HeightAtPoint(p, a, b, c, depth, h, d, cacheAtDepth);
                }
            }

            return new HeightMap(m);
        }

        public float HeightAtPoint(Vector2 p, Vertex a, Vertex b, Vertex c, int depth, float h, float d, int cacheAtDepth = -1)
        {
            iterations++;

            if (cacheAtDepth == -1)
                cacheAtDepth = this.cacheAtDepth;

            if (depth < 1)
            {
                return (a.h + b.h + c.h) / 3;
            }

            Triangle triangle = new Triangle(a, b, c);

            if (depth > cacheAtDepth)
            {
                foreach(Triangle t in cachedVertices)
                {
                    if (PointInTriangle(p, t.a.xz, t.b.xz, t.c.xz))
                    {
                        usedCaching++;
                        return HeightAtPoint(p, t.a, t.b, t.c, cacheAtDepth, h, d, -1);
                    }
                }
            }

            if (depth == cacheAtDepth)
            {
                if (!cachedVerticesHS.Contains(triangle))
                {
                    cachedVertices.Add(triangle);
                    cachedVerticesHS.Add(triangle);
                }
            }

            float newheight;
            float r;
            float distance = (a.xz - b.xz).magnitude;

            r = Noise.Value(a.r, b.r);
            newheight = d * r * (a.xz-b.xz).magnitude + Mathf.Abs(a.h - b.h) * h * r + (a.h + b.h) / 2;
            if (newheight < -1) newheight = -1;
            if (newheight > 1) newheight = 1;
            Vertex ab = new Vertex((a.xz + b.xz) / 2, newheight, r);

            r = Noise.Value(a.r, c.r);
            newheight = d * r * (a.xz - c.xz).magnitude + Mathf.Abs(a.h - c.h) * h * r + (a.h + c.h) / 2;
            if (newheight < -1) newheight = -1;
            if (newheight > 1) newheight = 1;
            Vertex ac = new Vertex((a.xz + c.xz) / 2, newheight, r);

            r = Noise.Value(b.r, c.r);
            newheight = d * r * (b.xz - c.xz).magnitude + Mathf.Abs(b.h - c.h) * h * r + (b.h + c.h) / 2;
            if (newheight < -1) newheight = -1;
            if (newheight > 1) newheight = 1;
            Vertex bc = new Vertex((b.xz + c.xz) / 2, newheight, r);

            if (PointInTriangle(p, a.xz, ab.xz, ac.xz))
            {
                return HeightAtPoint(p, a, ab, ac, depth - 1, h, d, cacheAtDepth);
            }

            if (PointInTriangle(p, b.xz, ab.xz, bc.xz))
            {
                return HeightAtPoint(p, ab, b, bc, depth - 1, h, d, cacheAtDepth);
            }

            if (PointInTriangle(p, ac.xz, c.xz, bc.xz))
            {
                return HeightAtPoint(p, ac, bc, c, depth - 1, h, d, cacheAtDepth);
            }

            if (PointInTriangle(p, ab.xz, bc.xz, ac.xz))
            {
                return HeightAtPoint(p, ab, bc, ac, depth - 1, h, d, cacheAtDepth);
            }

            return -1;
        }

        /// <summary>
        /// https://stackoverflow.com/questions/2049582/how-to-determine-if-a-point-is-in-a-2d-triangle
        /// </summary>
        private static bool PointInTriangle(Vector2 s, Vector2 a, Vector2 b, Vector2 c)
        {
            float as_x = s.x - a.x;
            float as_y = s.y - a.y;

            bool s_ab = (b.x - a.x) * as_y - (b.y - a.y) * as_x > 0;

            if ((c.x - a.x) * as_y - (c.y - a.y) * as_x > 0 == s_ab) return false;

            if ((c.x - b.x) * (s.y - b.y) - (c.y - b.y) * (s.x - b.x) > 0 != s_ab) return false;

            return true;
        }
    }
}