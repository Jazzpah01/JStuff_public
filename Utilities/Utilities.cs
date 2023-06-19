using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.Collections;
using System.Linq;

namespace JStuff.Utilities
{
    public static class Utilities
    {
        public static int[,] Fold(int[,] a1, int[,] a2)
        {
            if (a1.GetLength(0) != a2.GetLength(0) || a1.GetLength(1) != a2.GetLength(1))
                throw new System.Exception("Arrays are not same height or width.");

            int[,] retval = new int[a1.GetLength(0), a1.GetLength(1)];
            for (int i = 0; i < a1.GetLength(0); i++)
            {
                for (int j = 0; j < a1.GetLength(1); j++)
                {
                    retval[i, j] = a1[i, j] + a2[i, j];
                }
            }
            return retval;
        }

        public static int[,] Add(int[,] a, int number)
        {
            int[,] retval = new int[a.GetLength(0), a.GetLength(1)];
            for (int i = 0; i < a.GetLength(0); i++)
            {
                for (int j = 0; j < a.GetLength(1); j++)
                {
                    retval[i, j] = a[i, j] + number;
                }
            }
            return retval;
        }

        public static int[,] Copy(this int[,] a)
        {
            int[,] retval = new int[a.GetLength(0), a.GetLength(1)];
            for (int i = 0; i < a.GetLength(0); i++)
            {
                for (int j = 0; j < a.GetLength(1); j++)
                {
                    retval[i, j] = a[i, j];
                }
            }
            return retval;
        }

        public static int[,] ToInt(this float[,] map)
        {
            int[,] retval = new int[map.GetLength(0), map.GetLength(1)];
            for (int x = 0; x < retval.GetLength(0); x++)
            {
                for (int y = 0; y < retval.GetLength(1); y++)
                {
                    retval[x, y] = Mathf.FloorToInt((map[x, y] + 1) * 100);
                }
            }
            return retval;
        }

        public static float GetWeightOfPrefab(this IList<WeightedPrefab> list, GameObject prefab)
        {
            foreach (WeightedPrefab item in list)
            {
                if (item.prefab == prefab)
                    return item.weight;
            }
            return 0;
        }

        public static bool ContainsPrefab(this IList<WeightedPrefab> list, GameObject prefab)
        {
            foreach (WeightedPrefab item in list)
            {
                if (item.prefab == prefab)
                    return true;
            }
            return false;
        }

        public static IWeightedPrefab GetWeightedPrefab(this IList<IWeightedPrefab> list, System.Random rng)
        {
            if (list.Count == 0)
                throw new System.Exception("List is empty exception.");

            if (list.Count == 1)
                return list[0];

            float totalWeight = list.Aggregate(0f, (acc, elm) => acc + elm.Weight);

            float r = ((float)rng.NextDouble()) * totalWeight;
            float acc = 0;
            IWeightedPrefab retval = list[0];

            for (int i = 0; i < list.Count; i++)
            {
                acc += list[i].Weight;
                if (r < acc)
                {
                    return list[i];
                }
            }

            return retval;
        }

        public static GameObject GetRandomPrefab(this List<IWeightedPrefab> list, System.Random rng)
        {
            if (list.Count == 1)
                return list[0].Prefab;

            float totalWeight = list.Aggregate(0f, (acc, elm) => acc + elm.Weight);

            float r = ((float)rng.NextDouble()) * totalWeight;
            float acc = 0;
            IWeightedPrefab retval = list[0];

            for (int i = 0; i < list.Count; i++)
            {
                acc += list[i].Weight;
                if (r < acc)
                {
                    return list[i].Prefab;
                }
            }

            return retval.Prefab;
        }

        public static GameObject GetRandomPrefab(this IList<IWeightedPrefab> list, System.Random rng)
        {
            if (list.Count == 1)
                return list[0].Prefab;

            float totalWeight = list.Aggregate(0f, (acc, elm) => acc + elm.Weight);

            float r = ((float)rng.NextDouble()) * totalWeight;
            float acc = 0;
            IWeightedPrefab retval = list[0];

            for (int i = 0; i < list.Count; i++)
            {
                acc += list[i].Weight;
                if (r < acc)
                {
                    return list[i].Prefab;
                }
            }

            return retval.Prefab;
        }

        public static GameObject GetRandomPrefab(this IList<WeightedPrefab> list)
        {
            if (list.Count == 1)
                return list[0].prefab;

            float totalWeight = list.Aggregate(0f, (acc, elm) => acc + elm.weight);

            float r = (float)UnityEngine.Random.value * totalWeight;
            float acc = 0;
            WeightedPrefab retval = list[0];

            for (int i = 0; i < list.Count; i++)
            {
                acc += list[i].weight;
                if (r < acc)
                {
                    return list[i].prefab;
                }
            }

            return retval.prefab;
        }

        public static GameObject GetRandomPrefab(this List<WeightedPrefab> list)
        {
            if (list.Count == 1)
                return list[0].prefab;

            float totalWeight = list.Aggregate(0f, (acc, elm) => acc + elm.weight);

            float r = (float)UnityEngine.Random.value * totalWeight;
            float acc = 0;
            WeightedPrefab retval = list[0];

            for (int i = 0; i < list.Count; i++)
            {
                acc += list[i].weight;
                if (r < acc)
                {
                    return list[i].prefab;
                }
            }

            return retval.prefab;
        }

        // https://forum.unity.com/threads/re-map-a-number-from-one-range-to-another.119437/
        public static float Remap(this float value, float from1, float to1, float from2, float to2)
        {
            return ((float)value - (float)from1) / ((float)to1 - (float)from1) * ((float)to2 - (float)from2) + (float)from2;
        }

        public static float Clamp(this float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        public static int Clamp(this int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        public static float FractionalDigits(this float value)
        {
            return value - Mathf.Floor(value);
        }

        public static bool InRange(this int i, int from, int to)
        {
            if (i <= from) return false;
            if (i >= to) return false;
            return true;
        }

        // https://stackoverflow.com/questions/1014005/how-to-populate-instantiate-a-c-sharp-array-with-a-single-value
        public static void Populate<T>(this T[] arr, T value)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = value;
            }
        }

        //public static float OrientationXZ(this Vector3 v)
        //{
        //    float orientation = Vector3.Angle(new Vector3(1, 0, 0), v);

        //    float dot = Vector3.Dot(new Vector3(0, 0, 1), v);

        //    if (dot < 0)
        //    {
        //        return (380 - orientation);
        //    }

        //    return orientation;
        //}

        // Signed difference between two orientations
        public static float OrientationDifference(float source, float target)
        {
            return Mathf.Atan2(Mathf.Sin(target * 0.01745329252f - source * 0.01745329252f), 
                Mathf.Cos(target * 0.01745329252f - source * 0.01745329252f)) / 0.01745329252f;
        }

        public static float GetOrientation(this Vector2 v)
        {
            v = v.normalized;
            float orientation = Vector2.Angle(new Vector2(1, 0), v);

            float dot = Vector2.Dot(new Vector3(0, 1), v);

            if (dot < 0)
            {
                return 360 - orientation;
            }

            return orientation;
        }

        public static float GetOrientation(this Vector3 v, UpAxis up)
        {
            if (up == UpAxis.Y)
            {
                return GetOrientation(v);
            }
            else
            {
                return v.GetOrientation3D();
            }
        }

        private static float GetOrientation3D(this Vector3 v)
        {
            float orientation = Vector2.Angle(new Vector3(1, 0, 0), v);

            float dot = Vector2.Dot(new Vector3(0, 0, 1), v);

            if (dot < 0)
            {
                return 360 - orientation;

            }
            return orientation;
        }

        public static Vector3 GetDirection(this float orientation, UpAxis up)
        {
            Quaternion rotation = Quaternion.identity;

            if (up == UpAxis.Y)
            {
                rotation = Quaternion.Euler(0, orientation, 0);
            }
            else
            {
                rotation = Quaternion.Euler(0, 0, orientation);
            }

            Matrix4x4 m = Matrix4x4.Rotate(rotation);
            return (m * Vector3.right).normalized;
        }

        public static Vector2 GetDirection(this float f)
        {
            Quaternion rotation = Quaternion.identity;

            rotation = Quaternion.Euler(0, 0, f);

            Matrix4x4 m = Matrix4x4.Rotate(rotation);
            return (m * Vector3.right).normalized;
        }

        public static float DeltaOrientation(float currentOrientation, float targetOrientation)
        {
            return (targetOrientation - currentOrientation + 540) % 360 - 180;
        }

        public static float MoveTo(this float v, float target, float speed)
        {
            if (v > target)
            {
                return Mathf.Max(v - speed, target);
            } else if (v < target)
            {
                return Mathf.Min(v + speed, target);
            } else
            {
                return target;
            }
        }

        public static bool IsDestroyed(this object o)
        {
            return !(o as UnityEngine.Object);
        }
    }
}