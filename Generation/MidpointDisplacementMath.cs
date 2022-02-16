using UnityEngine;
using System;
using System.Collections;

namespace JStuff.Generation
{
    public struct Vertex : IEquatable<Vertex>
    {
        public Vertex(Vector2 v, float h, float r)
        {
            this.v = v;
            this.h = h;
            this.r = r;
        }
        public Vector2 v;
        public float h;
        public float r;

        public bool Equals(Vertex other)
        {
            if (v != other.v)
                return false;
            if (r != other.r)
                return false;
            if (h != other.h)
                return false;
            return true;
        }
    }

    public struct Edge
    {
        public Edge(bool river, float h)
        {
            this.river = river;
            this.h = h;
        }
        public bool river;
        public float h;
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
}