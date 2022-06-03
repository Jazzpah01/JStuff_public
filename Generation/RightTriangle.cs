using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.Random;

namespace JStuff.Generation
{
    public class RightTriangle
    {
        Vertex a;
        Vertex b;
        Vertex c;

        float heightFactor;
        float distanceFactor;

        float zoom;
        Vector2 offset;

        int size;

        public RightTriangle(int size, float heightFactor, float distanceFactor, float zoom, Vector2 offset)
        {
            this.size = size;
            this.heightFactor = heightFactor;
            this.distanceFactor = distanceFactor;
            this.zoom = zoom;
            this.offset = offset;

            a = new Vertex(0f, 1f, 0f, 0.4321558372f);
            b = new Vertex(0f, 0f, 0f, -0.9770926823f);
            c = new Vertex(1f, 0f, 0f, 0.2236396734f);
        }

        public HeightMap GetHeightMap(int depth)
        {
            float[,] m = new float[size, size];

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    Vector2 p = new Vector2(offset.x + (float)i / (float)(size - 1) / zoom,
                        offset.y + (float)j / (float)(size - 1) / zoom);

                    m[i, j] = HeightAtPoint(p, depth);
                }
            }

            return new HeightMap(m);
        }

        public float HeightAtPoint(Vector2 p, int depth)
        {
            Vertex a = this.a;
            Vertex b = this.b;
            Vertex c = this.c;

            int initialDepth = 0;

            if (!Mathg.PointInTriangle(p, a.xz, b.xz, c.xz))
            {
                return -1;
            }

            for (int i = initialDepth; i < depth; i++)
            {
                float r = Random.Generator.Value(a.r, c.r);
                float h = (a.h + c.h) / 2 + r * (a.xz - c.xz).magnitude * distanceFactor;
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

                //if (Mathg.PointInTriangle(p, ac.xz, a.xz, b.xz))
                //{
                //    c = b;
                //    b = ac;
                //}
                //else if (Mathg.PointInTriangle(p, ac.xz, b.xz, c.xz))
                //{
                //    a = c;
                //    c = b;
                //    b = ac;
                //}
                //else
                //{
                //    float distA = (p - a.xz).sqrMagnitude;
                //    float distC = (p - c.xz).sqrMagnitude;
                //    if (distA < distC)
                //    {
                //        c = b;
                //        b = ac;
                //    } else
                //    {
                //        a = c;
                //        c = b;
                //        b = ac;
                //    }
                //}
            }

            return (a.h + b.h + c.h) / 3.0f;
        }
    }
}