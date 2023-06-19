using UnityEngine;
using System;
using System.Collections;

namespace JStuff.Generation
{
    public struct Vertex : IEquatable<Vertex>
    {
        public Vector2 xz;
        public float h;
        public float r;

        public Vertex(Vector2 xz, float h, float r)
        {
            this.xz = xz;
            this.h = h;
            this.r = r;
        }
        public Vertex(float x, float z, float h, float r)
        {
            this.xz = new Vector2(x, z);
            this.h = h;
            this.r = r;
        }

        public bool Equals(Vertex other)
        {
            if (xz != other.xz)
                return false;
            if (r != other.r)
                return false;
            if (h != other.h)
                return false;
            return true;
        }

        public override string ToString()
        {
            return $"Vertex: ({xz.x},{xz.y},{h},{r})";
        }
    }

    public struct Triangle : IEquatable<Triangle>
    {
        public Triangle(Vertex a, Vertex b, Vertex c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }
        public Vertex a;
        public Vertex b;
        public Vertex c;

        public bool PointInTriangle(Vector2 s)
        {
            float as_x = s.x - a.xz.x;
            float as_y = s.y - a.xz.y;

            bool s_ab = (b.xz.x - a.xz.x) * as_y - (b.xz.y - a.xz.y) * as_x > 0;

            if ((c.xz.x - a.xz.x) * as_y - (c.xz.y - a.xz.y) * as_x > 0 == s_ab) return false;

            if ((c.xz.x - b.xz.x) * (s.y - b.xz.y) - (c.xz.y - b.xz.y) * (s.x - b.xz.x) > 0 != s_ab) return false;

            return true;
        }

        public bool Equals(Triangle other)
        {
            if (!a.Equals(other.a))
                return false;
            if (!b.Equals(other.b))
                return false;
            if (!c.Equals(other.c))
                return false;
            return true;
        }

        public override int GetHashCode()
        {
            return Mathf.FloorToInt((a.h + b.h + c.h) * 10000);
        }
    }

    public static class Mathg
    {
        /// <summary>
        /// https://stackoverflow.com/questions/2049582/how-to-determine-if-a-point-is-in-a-2d-triangle
        /// </summary>
        public static bool PointInTriangle(Vector2 s, Vector2 a, Vector2 b, Vector2 c)
        {
            float as_x = s.x - a.x;
            float as_y = s.y - a.y;

            bool s_ab = (b.x - a.x) * as_y - (b.y - a.y) * as_x > 0;

            if ((c.x - a.x) * as_y - (c.y - a.y) * as_x > 0 == s_ab) return false;

            if ((c.x - b.x) * (s.y - b.y) - (c.y - b.y) * (s.x - b.x) > 0 != s_ab) return false;

            return true;
        }

        public static float Distance(Vertex a, Vertex b)
        {
            return Vector2.Distance(a.xz, b.xz);
        }
    }
}