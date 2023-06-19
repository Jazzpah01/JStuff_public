using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.Generation
{
    public class RightTriangle
    {
        Vertex a;
        Vertex b;
        Vertex c;
        Vertex d;

        float heightFactor;
        float distanceFactor;

        float zoom;
        Vector2 offset;

        int size;

        public RightTriangle(int seed, int size, float heightFactor, float distanceFactor, float zoom, Vector2 offset)
        {
            System.Random rng = new System.Random(seed);

            this.size = size;
            this.heightFactor = heightFactor;
            this.distanceFactor = distanceFactor;
            this.zoom = zoom;
            this.offset = offset;

            a = new Vertex(0f, 1f, ((float)rng.NextDouble()) * 2f - 1f, ((float)rng.NextDouble()) * 2f - 1f);
            b = new Vertex(0f, 0f, ((float)rng.NextDouble()) * 2f - 1f, ((float)rng.NextDouble()) * 2f - 1f);
            c = new Vertex(1f, 0f, ((float)rng.NextDouble()) * 2f - 1f, ((float)rng.NextDouble()) * 2f - 1f);
            d = new Vertex(1f, 1f, ((float)rng.NextDouble()) * 2f - 1f, ((float)rng.NextDouble()) * 2f - 1f);
        }

        public RightTriangle(int seed, int size, float heightFactor, float distanceFactor, 
            float zoom, Vertex a, Vertex b, Vertex c, Vertex d)
        {
            this.size = size;
            this.heightFactor = heightFactor;
            this.distanceFactor = distanceFactor;
            this.zoom = zoom;
            this.offset = Vector2.zero;

            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
        }

        public HeightMap GetHeightMap(int depth = 0, bool zoomable = true)
        {
            float[,] m = new float[size, size];

            if (zoomable)
            {
                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < size; j++)
                    {
                        Vector2 p = new Vector2(offset.x + (float)i / (float)(size - 1) / zoom,
                            offset.y + (float)j / (float)(size - 1) / zoom);

                        m[i, j] = HeightAtPoint(p, depth);
                    }
                }
            } else
            {
                float conSize = 1 / zoom;

                Vertex[,] v = new Vertex[size, size];

                void Function(int ax, int ay, int bx, int by, int cx, int cy)
                {
                    if (Mathf.Abs(ax - cx) > 1 || Mathf.Abs(ay - cy) > 1)
                    {
                        int newbx = (ax + cx) / 2;
                        int newby = (ay + cy) / 2;

                        v[newbx, newby] = Noise.GetVertex(v[ax, ay], v[cx, cy], distanceFactor, heightFactor);
                        m[newbx, newby] = v[newbx, newby].h;

                        Function(ax, ay, newbx, newby, bx, by);
                        Function(bx, by, newbx, newby, cx, cy);
                    }
                }

                if (depth > 0)
                {
                    v[size - 1, 0] = VertexAtPoint(offset + new Vector2(1, 0) / zoom, depth);
                    v[0, 0] = VertexAtPoint(offset + new Vector2(0, 0) / zoom, depth);
                    v[0, size - 1] = VertexAtPoint(offset + new Vector2(0, 1) / zoom, depth);
                    v[size - 1, size - 1] = VertexAtPoint(offset + new Vector2(1, 1) / zoom, depth);
                }
                else
                {
                    //v[size - 1, 0] = a;
                    //v[0, 0] = b;
                    //v[0, size - 1] = c;
                    //v[size - 1, size - 1] = d;
                    v[0, size - 1] = a;
                    v[0, 0] = b;
                    v[size - 1, 0] = c;
                    v[size - 1, size - 1] = d;
                    //Debug.Log(a);
                    //Debug.Log(b);
                    //Debug.Log(c);
                    //Debug.Log(d);
                }

                m[0, size - 1] = v[0, size - 1].h;
                m[0, 0] = v[0, 0].h;
                m[size - 1, 0] = v[size - 1, 0].h;
                m[size - 1, size - 1] = v[size - 1, size - 1].h;

                Function(0, size - 1, 0, 0, size - 1, 0);
                Function(0, size - 1, size - 1, size - 1, size - 1, 0);
            }

            return new HeightMap(m);
        }

        public bool Inside(Vector2 p)
        {
            return Mathg.PointInTriangle(p, a.xz, b.xz, c.xz);
        }

        public float HeightAtPoint(Vector2 p, int depth)
        {
            Vertex a = this.a;
            Vertex b = this.b;
            Vertex c = this.c;

            int initialDepth = 0;

            if (Mathg.PointInTriangle(p, a.xz, d.xz, c.xz))
            {
                b = d;
            }
            else
            if (!Mathg.PointInTriangle(p, a.xz, b.xz, c.xz))
            {
                return -1;
            }

            for (int i = initialDepth; i < depth; i++)
            {
                float r = Noise.Value(a.r, c.r);
                float h = (a.h + c.h) / 2 + (a.xz - c.xz).magnitude * r * distanceFactor + Mathf.Abs(a.h - c.h) * r * heightFactor;
                Vertex ac = new Vertex((a.xz + c.xz) / 2.0f, h, r);

                float distA = (p - a.xz).sqrMagnitude;
                float distC = (p - c.xz).sqrMagnitude;
                if (distA < distC)
                {
                    c = b;
                    b = ac;
                }
                else if (distA > distC)
                {
                    a = c;
                    c = b;
                    b = ac;
                }
                else if (a.r > c.r)
                {
                    c = b;
                    b = ac;
                }
                else
                {
                    a = c;
                    c = b;
                    b = ac;
                }
            }

            return (a.h + b.h + c.h) / 3.0f;
        }

        public Vertex VertexAtPoint(Vector2 p, int depth)
        {
            Vertex a = this.a;
            Vertex b = this.b;
            Vertex c = this.c;

            int initialDepth = 0;

            if (Mathg.PointInTriangle(p, a.xz, d.xz, c.xz))
            {
                b = d;
            }
            else
            if (!Mathg.PointInTriangle(p, a.xz, b.xz, c.xz))
            {
                return new Vertex(0, 0, 0, 0);
            }

            for (int i = initialDepth; i < depth; i++)
            {
                float r = Noise.Value(a.r, c.r);
                float h = (a.h + c.h) / 2 + (a.xz - c.xz).magnitude * r * distanceFactor + Mathf.Abs(a.h - c.h) * r * heightFactor;
                Vertex ac = new Vertex((a.xz + c.xz) / 2.0f, h, r);

                float distA = (p - a.xz).sqrMagnitude;
                float distC = (p - c.xz).sqrMagnitude;
                if (distA < distC)
                {
                    c = b;
                    b = ac;
                }
                else if (distA > distC)
                {
                    a = c;
                    c = b;
                    b = ac;
                }
                else if (a.r > c.r)
                {
                    c = b;
                    b = ac;
                }
                else
                {
                    a = c;
                    c = b;
                    b = ac;
                }
            }

            return a;
        }
    }
}