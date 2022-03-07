using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.Random;
using System;

namespace JStuff.Generation
{
    public static class PointPlacement
    {
        public static List<Vector2> GetPoints(float size, float pointRadius, int amount, float r1, bool ignoreRadius = false)
        {
            float s = pointRadius / Mathf.Sqrt(2);
            int array_size = Mathf.CeilToInt(size / s);
            Vector2[,] pointOf = new Vector2[array_size, array_size];
            List<Vector2> points = new List<Vector2>();

            for (int i = 0; i < pointOf.GetLength(0); i++)
            {
                for (int j = 0; j < pointOf.GetLength(0); j++)
                {
                    pointOf[i, j] = Vector2.zero;
                }
            }

            float r2 = Generator.NormalValue(r1, r1 * r1);
            float r3 = Generator.NormalValue(r1, r2);

            for (int i = 0; i < amount; i++)
            {
                Vector2 newpoint = new Vector2(r2, r3) * size;
                r1 = 1 - Generator.NormalValue(r2, r3);
                r2 = Generator.NormalValue(r1, r3);
                r3 = Generator.NormalValue(r1, r2);
                if ((ignoreRadius && LegalPoint_IgnoreRadius(pointOf, newpoint, pointRadius, size)) || LegalPoint(pointOf, newpoint, pointRadius, size))
                {
                    points.Add(newpoint);
                    pointOf[Mathf.FloorToInt(newpoint.x.Remap(0, size, 0, array_size - 1)),
                            Mathf.FloorToInt(newpoint.y.Remap(0, size, 0, array_size - 1))] = newpoint;
                }
            }

            return points;
        }

        public static List<Vector2> GetUnfilteredPoints(float size, int amount, float r1)
        {
            List<Vector2> points = new List<Vector2>();

            float r2 = Generator.NormalValue(r1, r1 * r1);
            float r3 = Generator.NormalValue(r1, r2);

            for (int i = 0; i < amount; i++)
            {
                points.Add(new Vector2(r2, r3) * size);
                r1 = Generator.NormalValue(r2, r3);
                r2 = Generator.NormalValue(r1, r3);
                r3 = Generator.NormalValue(r1, r2);
            }

            return points;
        }

        private static bool LegalPoint(Vector2[,] pointOf, Vector2 point, float pointRadius, float size)
        {
            bool retval = true;

            float s = pointRadius / Mathf.Sqrt(2);
            int array_size = pointOf.GetLength(0);
            int x = Mathf.FloorToInt(point.x.Remap(0, size, 0, array_size - 1));
            int y = Mathf.FloorToInt(point.y.Remap(0, size, 0, array_size - 1));
            for (int i = -2; i < 3; i++)
            {
                for (int j = -2; j < 3; j++)
                {
                    if (j + y >= 0 && j + y < array_size && i + x >= 0 && i + x < array_size)
                    {
                        if (pointOf[x + i, y + j] != Vector2.zero &&
                            (pointOf[x + i, y + j] - point == Vector2.zero ||
                            (pointOf[x + i, y + j] - point).magnitude < pointRadius * 2))
                        {
                            retval = false;
                        }
                    }
                }
            }
            return retval;
        }

        private static bool LegalPoint_IgnoreRadius(Vector2[,] pointOf, Vector2 point, float pointRadius, float size)
        {
            bool retval = true;

            float s = pointRadius / Mathf.Sqrt(2);
            int array_size = pointOf.GetLength(0);
            int x = Mathf.FloorToInt(point.x.Remap(0, size, 0, array_size - 1));
            int y = Mathf.FloorToInt(point.y.Remap(0, size, 0, array_size - 1));
            for (int i = -2; i < 3; i++)
            {
                for (int j = -2; j < 3; j++)
                {
                    if (j + y >= 0 && j + y < array_size && i + x >= 0 && i + x < array_size)
                    {
                        if (pointOf[x + i, y + j] - point == Vector2.zero)
                        {
                            retval = false;
                        }
                    }
                }
            }
            return retval;
        }
    }
}