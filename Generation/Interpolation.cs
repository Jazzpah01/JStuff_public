using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.Generation
{
    public static class Interpolation
    {
        /// <summary>
        /// Value interpolation over a specified function. Map f(t) to [a0,a1], where t is [0,1] and f(t) is [0,1].
        /// </summary>
        /// <param name="a0"></param>
        /// <param name="a1"></param>
        /// <param name="t">Value between 0 and 1.</param>
        /// <param name="f">Function where f(t) is [0,1] when t is [0,1]</param>
        /// <returns></returns>
        public static float ValueInterpolation(float a0, float a1, float t, System.Func<float, float> f)
        {
            return UnitLerp(a0, a1, f(t));
        }

        /// <summary>
        /// Linear value interpolation. Map t to [a0,a1], where t is [0,1].
        /// </summary>
        /// <param name="a0"></param>
        /// <param name="a1"></param>
        /// <param name="t">Value between 0 and 1.</param>
        /// <returns></returns>
        public static float ValueInterpolation(float a0, float a1, float t)
        {
            return UnitLerp(a0, a1, t);
        }

        public static float UnitClamp(float x)
        {
            return (x < 0) ? ((x > 1) ? 1 : x) : 0;
        }

        public static float UnitWrap(float x)
        {
            return x - Mathf.Floor(x);
        }

        public static float UnitStep(float x)
        {
            return (x > 0) ? 1 : 0;
        }

        public static float UnitRescale(float min, float max, float x)
        {
            if (max < min || (x < min || x > max))
                throw new System.Exception("x must be between min and max and max must be larger than min.");

            return (x - min) / (max - min);
        }

        public static float UnitLerp(float a0, float a1, float t)
        {
            return a0 * (1 - t) + a1 * t;
        }



        public static float[,] InterpolateIntMap(int[,] map, int detail, System.Func<float, float> function)
        {
            float[,] retval = new float[map.GetLength(0) * detail - detail, map.GetLength(1) * detail - detail];

            //6t^5 - 15t^4 + 10t^3
            //t

            for (int x = 0; x < retval.GetLength(0); x++)
            {
                for (int y = 0; y < retval.GetLength(1); y++)
                {
                    float x_ = (float)x / detail;
                    float y_ = (float)y / detail;

                    float a00 = map[Mathf.FloorToInt(x_), Mathf.FloorToInt(y_)];
                    float a10 = map[Mathf.CeilToInt(x_), Mathf.FloorToInt(y_)];
                    float a01 = map[Mathf.FloorToInt(x_), Mathf.CeilToInt(y_)];
                    float a11 = map[Mathf.CeilToInt(x_), Mathf.CeilToInt(y_)];

                    retval[x, y] = Interpolation.ValueInterpolation(
                        Interpolation.ValueInterpolation(a00, a10, (float)(x % detail) / detail, function),
                        Interpolation.ValueInterpolation(a01, a11, (float)(x % detail) / detail, function),
                        (float)(y % detail) / detail, function);
                }
            }

            return retval;
        }
        public static float[,] InterpolateIntMap(float[,] map, int detail, System.Func<float, float> function)
        {
            float[,] retval = new float[map.GetLength(0) * detail - detail, map.GetLength(1) * detail - detail];

            //6t^5 - 15t^4 + 10t^3
            //t

            for (int x = 0; x < retval.GetLength(0); x++)
            {
                for (int y = 0; y < retval.GetLength(1); y++)
                {
                    float x_ = (float)x / detail;
                    float y_ = (float)y / detail;

                    float a00 = map[Mathf.FloorToInt(x_), Mathf.FloorToInt(y_)];
                    float a10 = map[Mathf.CeilToInt(x_), Mathf.FloorToInt(y_)];
                    float a01 = map[Mathf.FloorToInt(x_), Mathf.CeilToInt(y_)];
                    float a11 = map[Mathf.CeilToInt(x_), Mathf.CeilToInt(y_)];

                    retval[x, y] = Interpolation.ValueInterpolation(
                        Interpolation.ValueInterpolation(a00, a10, (float)(x % detail) / detail, function),
                        Interpolation.ValueInterpolation(a01, a11, (float)(x % detail) / detail, function),
                        (float)(y % detail) / detail, function);
                }
            }

            return retval;
        }
    }
}