using JStuff.Random;
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

        public float HeightAtPoint(Vector2 p, Vertex a, Vertex b, Vertex c, int depth, Func<Vertex, Vertex, float, float> heightFunction, int cacheAtDepth = -1)
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
                    if (PointInTriangle(p, t.a.v, t.b.v, t.c.v))
                    {
                        usedCaching++;
                        return HeightAtPoint(p, t.a, t.b, t.c, cacheAtDepth, heightFunction, -1);
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
            float distance = (a.v - b.v).magnitude;


            r = Generator.Value(a.r, b.r);
            newheight = heightFunction(a, b, r);
            if (newheight < -1) newheight = -1;
            if (newheight > 1) newheight = 1;
            Vertex ab = new Vertex((a.v + b.v) / 2, newheight, r);

            r = Generator.Value(a.r, c.r);
            newheight = heightFunction(a, c, r);
            if (newheight < -1) newheight = -1;
            if (newheight > 1) newheight = 1;
            Vertex ac = new Vertex((a.v + c.v) / 2, newheight, r);

            r = Generator.Value(b.r, c.r);
            newheight = heightFunction(b, c, r);
            if (newheight < -1) newheight = -1;
            if (newheight > 1) newheight = 1;
            Vertex bc = new Vertex((b.v + c.v) / 2, newheight, r);

            if (PointInTriangle(p, a.v, ab.v, ac.v))
            {
                return HeightAtPoint(p, a, ab, ac, depth - 1, heightFunction, cacheAtDepth);
            }

            if (PointInTriangle(p, b.v, ab.v, bc.v))
            {
                return HeightAtPoint(p, ab, b, bc, depth - 1, heightFunction, cacheAtDepth);
            }

            if (PointInTriangle(p, ac.v, c.v, bc.v))
            {
                return HeightAtPoint(p, ac, bc, c, depth - 1, heightFunction, cacheAtDepth);
            }

            if (PointInTriangle(p, ab.v, bc.v, ac.v))
            {
                return HeightAtPoint(p, ab, bc, ac, depth - 1, heightFunction, cacheAtDepth);
            }

            return -1;
        }

        private static float HeightAtPointInter(Vector2 p, Vertex a, Vertex b, Vertex c, int depth, Func<float, float> funcd, Func<float, float> funch)
        {
            if (depth < 1)
            {
                return (a.h + b.h + c.h) / 3;
            }

            float newheight;
            float r;
            float distance = (a.v - b.v).magnitude;


            r = Generator.Value(a.r, b.r) - 0.05f;
            newheight = Interpolation.ValueInterpolation((a.h + b.h) / 2, r, distance, funcd);
            newheight = Interpolation.ValueInterpolation(newheight, r, Mathf.Abs(a.h - b.h), funch);
            Vertex ab = new Vertex((a.v + b.v) / 2, newheight, r);

            r = Generator.Value(a.r, c.r) - 0.05f;
            newheight = Interpolation.ValueInterpolation((a.h + c.h) / 2, r, distance, funcd);
            newheight = Interpolation.ValueInterpolation(newheight, r, Mathf.Abs(a.h - c.h), funch);
            Vertex ac = new Vertex((a.v + c.v) / 2, newheight, r);

            r = Generator.Value(b.r, c.r) - 0.05f;
            newheight = Interpolation.ValueInterpolation((b.h + c.h) / 2, r, distance, funcd);
            newheight = Interpolation.ValueInterpolation(newheight, r, Mathf.Abs(b.h - c.h), funch);
            Vertex bc = new Vertex((b.v + c.v) / 2, newheight, r);

            if (PointInTriangle(p, a.v, ab.v, ac.v))
            {
                return HeightAtPointInter(p, a, ab, ac, depth - 1, funcd, funch);
            }

            if (PointInTriangle(p, b.v, ab.v, bc.v))
            {
                return HeightAtPointInter(p, ab, b, bc, depth - 1, funcd, funch);
            }

            if (PointInTriangle(p, ac.v, c.v, bc.v))
            {
                return HeightAtPointInter(p, ac, bc, c, depth - 1, funcd, funch);
            }

            if (PointInTriangle(p, ab.v, bc.v, ac.v))
            {
                return HeightAtPointInter(p, ab, bc, ac, depth - 1, funcd, funch);
            }

            return -1;
        }

        private static float HeightAtPointC(Vector2 p, Vertex a, Vertex b, Vertex c, float heightVar, float distVar, int depth)
        {
            if (depth < 1)
            {
                return (a.h + b.h + c.h) / 3;
            }

            float newheight;
            float r;
            float distance = (a.v - b.v).magnitude;


            r = Generator.Value(a.r, b.r);
            newheight = (a.h + b.h) / 2 + r * (distance * distVar + Mathf.Abs(a.h - b.h) * heightVar);
            if (newheight < -1) newheight = -1;
            if (newheight > 1) newheight = 1;
            Vertex ab = new Vertex((a.v + b.v) / 2, newheight, r);

            r = Generator.Value(a.r, c.r);
            newheight = (a.h + c.h) / 2 + r * (distance * distVar + Mathf.Abs(a.h - c.h) * heightVar);
            if (newheight < -1) newheight = -1;
            if (newheight > 1) newheight = 1;
            Vertex ac = new Vertex((a.v + c.v) / 2, newheight, r);

            r = Generator.Value(b.r, c.r);
            newheight = (b.h + c.h) / 2 + r * (distance * distVar + Mathf.Abs(b.h - c.h) * heightVar);
            if (newheight < -1) newheight = -1;
            if (newheight > 1) newheight = 1;
            Vertex bc = new Vertex((b.v + c.v) / 2, newheight, r);

            if (PointInTriangle(p, a.v, ab.v, ac.v))
            {
                return HeightAtPointC(p, a, ab, ac, heightVar, distVar, depth - 1);
            }

            if (PointInTriangle(p, b.v, ab.v, bc.v))
            {
                return HeightAtPointC(p, ab, b, bc, heightVar, distVar, depth - 1);
            }

            if (PointInTriangle(p, ac.v, c.v, bc.v))
            {
                return HeightAtPointC(p, ac, bc, c, heightVar, distVar, depth - 1);
            }

            if (PointInTriangle(p, ab.v, bc.v, ac.v))
            {
                return HeightAtPointC(p, ab, bc, ac, heightVar, distVar, depth - 1);
            }

            return -1;
        }



        private static float HeightAtPointRiver(Vector2 p, Vertex a, Vertex b, Vertex c, Edge ab_e, Edge bc_e, Edge ca_e, float heightVar, int depth, Func<float, float> func)
        {
            if (depth < 1)
            {
                return (a.h + b.h + c.h) / 3;
            }

            float r = Generator.Value(a.r, b.r);
            Vertex ab_v = new Vertex((a.v + b.v) / 2, (a.h + b.h) / 2 + r * (a.v - b.v).magnitude * heightVar, r);
            r = Generator.Value(a.r, c.r);
            Vertex ac_v = new Vertex((a.v + c.v) / 2, (a.h + c.h) / 2 + r * (a.v - b.v).magnitude * heightVar, r);
            r = Generator.Value(b.r, c.r);
            Vertex bc_v = new Vertex((b.v + c.v) / 2, (b.h + c.h) / 2 + r * (a.v - b.v).magnitude * heightVar, r);

            // River flow up or down

            // Generate river

            // Generate rivers between

            //if (PointInTriangle(p, a.v, ab_v.v, ac_v.v))
            //{
            //    return HeightAtPointRiver(p, a, ab_v, ac_v, heightVar, depth - 1, func);
            //}
            //
            //if (PointInTriangle(p, b.v, ab_v.v, bc_v.v))
            //{
            //    return HeightAtPointRiver(p, ab_v, b, bc_v, heightVar, depth - 1, func);
            //}
            //
            //if (PointInTriangle(p, ac_v.v, c.v, bc_v.v))
            //{
            //    return HeightAtPointRiver(p, ac_v, bc_v, c, heightVar, depth - 1, func);
            //}
            //
            //if (PointInTriangle(p, ab_v.v, bc_v.v, ac_v.v))
            //{
            //    return HeightAtPointRiver(p, ab_v, bc_v, ac_v, heightVar, depth - 1, func);
            //}

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