using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace JStuff.Utility
{
    public static class ListExtensions
    {
        /// <summary>
        /// Returns an element from a list randomly given the weight of the elements of the list.
        /// </summary>
        /// <typeparam name="T">Type of element in list.</typeparam>
        /// <param name="lst">The list.</param>
        /// <param name="r">A random value from 0 to 1 (inclusive).</param>
        /// <param name="weightFunction">A function that determines the weight of a list element.</param>
        /// <returns>An element from lst chosen randomly given weightFunction.</returns>
        public static T Choose<T>(this IReadOnlyList<T> lst, float r, Func<T, float> weightFunction)
        {
            if (lst == null || lst.Count == 0)
                throw new System.Exception("Cannot choose an element from an empty list.");

            float sum = 0;
            foreach (T item in lst)
            {
                sum += Mathf.Max(weightFunction(item), 0);
            }

            if (sum <= 0)
                throw new System.Exception("Cannot choose an element in list if no element has weight greater than 0.");

            r *= sum;
            sum = 0;
            T last = default;

            for (int i = 0; i < lst.Count; i++)
            {
                float weight = weightFunction(lst[i]);
                sum += Mathf.Max(weight, 0);
                if (weight > 0)
                    last = lst[i];
                if (r < sum)
                    return lst[i];
            }

            return last;
        }

        /// <summary>
        /// Returns an element from the list randomly given a uniform distribution.
        /// </summary>
        /// <typeparam name="T">Type of element in the list.</typeparam>
        /// <param name="lst">The list.</param>
        /// <param name="r">A random value from 0 to 1 (inclusive).</param>
        /// <returns>An element from lst chosen randomly from a uniform distribution.</returns>
        public static T Choose<T>(this IReadOnlyList<T> lst, float r)
        {
            if (lst == null || lst.Count == 0)
                throw new System.Exception("Cannot choose an element from an empty list.");

            float sum = lst.Count;

            r *= sum;
            sum = 0;
            T last = default;

            for (int i = 0; i < lst.Count; i++)
            {
                sum += 1;
                last = lst[i];
                if (r < sum)
                    return lst[i];
            }

            return last;
        }

        /// <summary>
        /// Returns a random index given the weights of the elements of the list.
        /// </summary>
        /// <param name="lst">List of weights.</param>
        /// <param name="r">A random value from 0 to 1 (inclusive).</param>
        /// <returns>A random index from the list.</returns>
        public static int ChooseIndex(this IReadOnlyList<float> lst, float r)
        {
            if (lst == null || lst.Count == 0)
                throw new System.Exception("Cannot choose an element from an empty list.");

            float sum = 0;
            foreach (float item in lst)
            {
                sum += Mathf.Max(item, 0);
            }

            if (sum <= 0)
                throw new System.Exception("Cannot choose an element in list if no element has weight greater than 0.");

            r *= sum;
            sum = 0;
            int last = default;

            for (int i = 0; i < lst.Count; i++)
            {
                sum += Mathf.Max(lst[i], 0);
                if (Mathf.Max(lst[i], 0) > 0)
                    last = i;
                if (r < sum)
                    return i;
            }

            return lst.Count - 1;
        }


        public static List<R> Map<T, R>(this IList<T> lst, Func<T, R> f)
        {
            List<R> retval = new List<R>();

            for (int i = 0; i < lst.Count; i++)
            {
                retval.Add(f(lst[i]));
            }

            return retval;
        }

        public static List<T> Filter<T>(this IList<T> lst, Func<T, bool> f)
        {
            List<T> retval = new List<T>();

            for (int i = 0; i < lst.Count; i++)
            {
                if (f(lst[i]))
                    retval.Add(lst[i]);
            }

            return retval;
        }

        public static R Reduce<T, R>(this IList<T> lst, Func<T, R, R> f, R r)
        {
            R retval = r;

            for (int i = 0; i < lst.Count; i++)
            {
                retval = f(lst[i], retval);
            }

            return retval;
        }
    }
}