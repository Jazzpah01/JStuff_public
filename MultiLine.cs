using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.Generation;

public class MultiLine
{
    List<Vertex> vertices = new List<Vertex>();
    List<(float, float)> presentChunks = new List<(float, float)>();
    float chunkSize;

    public MultiLine(float chunkSize)
    {
        this.chunkSize = chunkSize;
    }

    public Vertex this[int i]
    {
        get => vertices[i];
    }

    public int Length
    {
        get => vertices.Count;
    }

    public void AddVertex(Vertex v)
    {
        vertices.Add(v);
    }
    public void AddVertex(float x, float z)
    {
        Vertex v = new Vertex();
        v.xz.x = x;
        v.xz.y = z;

        (float, float) chunkpos = (x - x % chunkSize, z - z % chunkSize);

        if (!presentChunks.Contains(chunkpos)){
            presentChunks.Add((x - x % chunkSize, z - z % chunkSize));

            presentChunks.Add((x - x % chunkSize + chunkSize, z - z % chunkSize));
            presentChunks.Add((x - x % chunkSize + chunkSize, z - z % chunkSize));
            presentChunks.Add((x - x % chunkSize, z - z % chunkSize + chunkSize));
            presentChunks.Add((x - x % chunkSize, z - z % chunkSize + chunkSize));

            presentChunks.Add((x - x % chunkSize + chunkSize, z - z % chunkSize + chunkSize));
            presentChunks.Add((x - x % chunkSize + chunkSize, z - z % chunkSize - chunkSize));
            presentChunks.Add((x - x % chunkSize - chunkSize, z - z % chunkSize + chunkSize));
            presentChunks.Add((x - x % chunkSize - chunkSize, z - z % chunkSize - chunkSize));
        }

        vertices.Add(v);
    }

    public void AddVertex(Vector2 vector)
    {
        Vertex v = new Vertex();
        v.xz.x = vector.x;
        v.xz.y = vector.y;

        float x = vector.x;
        float z = vector.y;

        (float, float) chunkpos = (vector.x - vector.x % chunkSize, vector.y - vector.y % chunkSize);

        if (!presentChunks.Contains(chunkpos))
        {
            presentChunks.Add((x - x % chunkSize, z - z % chunkSize));

            presentChunks.Add((x - x % chunkSize + chunkSize, z - z % chunkSize));
            presentChunks.Add((x - x % chunkSize + chunkSize, z - z % chunkSize));
            presentChunks.Add((x - x % chunkSize, z - z % chunkSize + chunkSize));
            presentChunks.Add((x - x % chunkSize, z - z % chunkSize + chunkSize));

            presentChunks.Add((x - x % chunkSize + chunkSize, z - z % chunkSize + chunkSize));
            presentChunks.Add((x - x % chunkSize + chunkSize, z - z % chunkSize - chunkSize));
            presentChunks.Add((x - x % chunkSize - chunkSize, z - z % chunkSize + chunkSize));
            presentChunks.Add((x - x % chunkSize - chunkSize, z - z % chunkSize - chunkSize));
        }

        vertices.Add(v);
    }

    public void GenerateVertices(int a)
    {
        List<Vertex> nvertices = new List<Vertex>();

        for (int i = 0; i < vertices.Count - 1; i++)
        {
            nvertices.Add(vertices[i]);
            for (int j = 0; j < a; j++)
            {
                Vector2 v = vertices[i].xz + (vertices[i + 1].xz - vertices[i].xz) / a * j;

                Vertex vertex = new Vertex();

                vertex.xz = v;

                nvertices.Add(vertex);
            }
        }
    }

    public float Distance(Vector2 p)
    {
        (float, float) chunkpos = (p.x - p.x % chunkSize, p.y - p.y % chunkSize);

        if (!presentChunks.Contains(chunkpos))
            return Mathf.Infinity;

        float retval = float.PositiveInfinity;

        //foreach(Vertex v in vertices)
        //{
        //    float val = (v.v - p).magnitude;
        //    if (val < retval)
        //        retval = val;
        //}

        for (int i = 0; i < vertices.Count-1; i++)
        {
            float d = minimum_distance(vertices[i].xz, vertices[i + 1].xz, p);
            if (d < retval)
                retval = d;
        }

        return retval;
    }

    // https://stackoverflow.com/questions/849211/shortest-distance-between-a-point-and-a-line-segment
    float minimum_distance(Vector2 v, Vector2 w, Vector2 p)
    {
        // Return minimum distance between line segment vw and point p
        float l2 = (v - w).magnitude * (v - w).magnitude;  // i.e. |w-v|^2 -  avoid a sqrt
        if (l2 == 0.0) return (p - v).magnitude;   // v == w case
                                                // Consider the line extending the segment, parameterized as v + t (w - v).
                                                // We find projection of point p onto the line. 
                                                // It falls where t = [(p-v) . (w-v)] / |w-v|^2
                                                // We clamp t from [0,1] to handle points outside the segment vw.
        float t = Mathf.Max(0, Mathf.Min(1, Vector2.Dot(p - v, w - v) / l2));
        Vector2 projection = v + t * (w - v);  // Projection falls on the segment
        return (p - projection).magnitude;
    }

    float pDistance(Vector2 v, Vector2 w, Vector2 p)
    {
        float A = p.x - v.x;
        float B = p.y - v.y;
        float C = w.x - v.x;
        float D = w.y - w.y;

        //float A = x - x1;
        //float B = y - y1;
        //float C = x2 - x1;
        //float D = y2 - y1;

        float dot = A * C + B * D;
        float len_sq = C * C + D * D;
        float param = -1;
        if (len_sq != 0) //in case of 0 length line
            param = dot / len_sq;

        float xx, yy;

        if (param < 0)
        {
            xx = v.x;
            yy = v.y;
        }
        else if (param > 1)
        {
            xx = w.x;
            yy = w.y;
        }
        else
        {
            xx = v.x + param * C;
            yy = w.y + param * D;
        }

        var dx = p.x - xx;
        var dy = p.y - yy;
        return Mathf.Sqrt(dx * dx + dy * dy);
    }
}