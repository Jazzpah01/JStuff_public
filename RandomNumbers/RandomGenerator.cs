using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace JStuff.Random
{
    public static class Generator
    {
        private static int[] numbers;
        private static int index;

        /// <summary>
        /// Generate an array of numbers, in the interval [start;end].
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public static void GenerateNumbers(int start, int end, int amount)
        {
            numbers = new int[amount];
            index = 0;

            int m = 2147483647;
            int a = 16807;
            int c = 10211;
            int X = Mathf.Abs(DateTime.UtcNow.Second) % (m-1) + 1;

            for (int i = 0; i < amount; i++)
            {
                numbers[i] = (a * X + c) % m % (end - start) - start;
                X = numbers[i];
            }
        }

        public static int NextInteger()
        {
            int retval = numbers[index];
            if (numbers.Length <= index)
            {
                throw new System.Exception("Random numbers are exhausted!");
            }
            index++;
            return retval;
        }


        /// <summary>
        /// A random float from -1 to 1 (inclusive) generated deterministically from 2 input numbers.
        /// </summary>
        /// <param name="a">First float used as seed.</param>
        /// <param name="b">Second float used as seed.</param>
        /// <returns>A random value from -1 to 1.</returns>
        public static float Value(float a, float b)
        {
            float retval = (Mathf.PI + a) * (Mathf.PI + b);
            return retval.FractionalDigits() * 2 - 1;
        }

        /// <summary>
        /// A random float from 0 to 1 (inclusive) generated deterministically from 2 input numbers.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float NormalValue(float a, float b)
        {
            float retval = (Mathf.PI + Mathf.Abs(a)) * (Mathf.PI + Mathf.Abs(b));
            return retval.FractionalDigits();
        }
    }
}