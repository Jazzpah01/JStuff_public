using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using JStuff.AI.Pathfinding;
using JStuff.Random;

namespace JStuff.Generation
{
    public class HeightMap
    {
        float[,] map;

        /// <summary>
        /// Get length of second dimension.
        /// </summary>
        public int Length
        {
            get => map.GetLength(1);
        }

        /// <summary>
        /// Get length of first dimension.
        /// </summary>
        public int Width
        {
            get => map.GetLength(0);
        }
        public float this[int i, int j]
        {
            get { return map[i, j]; }
        }

        public float[,] Array => map;

        public HeightMap(float[,] map, bool forceScale = false, bool checkRange = true)
        {
            this.map = map;

            if (forceScale)
            {
                SetScale(checkBounds: true);
            }
            else if (checkRange)
            {
                for (int i = 0; i < map.GetLength(0); i++)
                {
                    for (int j = 0; j < map.GetLength(1); j++)
                    {
                        if (map[i, j] > 1)
                            map[i, j] = 1;
                        if (map[i, j] < -1)
                            map[i, j] = -1;
                    }
                }
            }
        }

        public HeightMap(int size, float elevation)
        {
            this.map = new float[size, size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    this.map[x, y] = elevation;
                }
            }
        }

        public HeightMap FilteredHeightMap(Func<float, float> func)
        {
            float[,] retval = new float[this.Width, this.Length];

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Width; y++)
                {
                    float f = this.map[x, y];
                    retval[x, y] = func(f);
                }
            }

            return new HeightMap(retval, checkRange: true);
        }

        public void SetScale(bool checkBounds = false)
        {
            float max = float.MinValue; float min = float.MaxValue;

            // Find max a min values
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    if (map[i, j] > max)
                        max = map[i, j];
                    if (map[i, j] < min)
                        min = map[i, j];
                }
            }

            // Scale heightmap, so that min is -1 and max is 1
            if (checkBounds && (max > 1 || min < -1))
            {
                for (int i = 0; i < map.GetLength(0); i++)
                {
                    for (int j = 0; j < map.GetLength(1); j++)
                    {
                        // Normalize values
                        map[i, j] = (map[i, j] - min) / (max - min) * 2 - 1;
                    }
                }
            }
        }


        public Texture2D ToTexture()
        {
            int wh = map.GetLength(0);

            Texture2D texture = new Texture2D(wh, wh);
            texture.filterMode = FilterMode.Point;

            Color[] colourmap = new Color[wh * wh];

            for (int y = 0; y < wh; y++)
            {
                for (int x = 0; x < wh; x++)
                {
                    colourmap[y * wh + x] = Color.Lerp(Color.black, Color.white, (map[x, y] + 1) / 2);
                }
            }

            texture.SetPixels(colourmap);
            texture.Apply();

            return texture;
        }

        public Color[] GetColorMap()
        {
            int wh = Width;

            Color[] colourmap = new Color[wh * wh];

            for (int y = 0; y < wh; y++)
            {
                for (int x = 0; x < wh; x++)
                {
                    float h = map[x, y];

                    colourmap[y * wh + x] = Color.Lerp(Color.black, Color.white, (map[x, y] + 1) / 2);
                }
            }

            return colourmap;
        }

        public Color[] GetColorMap(Gradient gradient)
        {
            int wh = Width;

            Color[] colourmap = new Color[wh * wh];

            for (int y = 0; y < wh; y++)
            {
                for (int x = 0; x < wh; x++)
                {
                    float h = map[x, y];

                    colourmap[y * wh + x] = gradient.Evaluate((h + 1) / 2);
                }
            }

            return colourmap;
        }

        public Texture2D ToTexture(ColorMapping cola)
        {
            int wh = map.GetLength(0);

            Texture2D texture = new Texture2D(wh, wh);
            texture.filterMode = FilterMode.Point;

            Color[] colourmap = new Color[wh * wh];

            for (int y = 0; y < wh; y++)
            {
                for (int x = 0; x < wh; x++)
                {
                    float t = 0;
                    float h = map[x, y];

                    Color upper = Color.white;
                    Color lower = Color.black;

                    for (int i = 0; i < cola.Length - 1; i++)
                    {
                        if (h >= cola[i].h && h <= cola[i + 1].h)
                        {
                            upper = cola[i + 1].c;
                            lower = cola[i].c;

                            t = (h - cola[i].h) / (cola[i + 1].h - cola[i].h);
                        }
                    }

                    colourmap[y * wh + x] = Color.Lerp(lower, upper, t);
                }
            }

            texture.SetPixels(colourmap);
            texture.Apply();

            return texture;
        }

        public Texture2D ToTexture(Gradient gradient)
        {
            int wh = map.GetLength(0);

            Texture2D texture = new Texture2D(wh, wh);
            texture.filterMode = FilterMode.Point;

            Color[] colourmap = new Color[wh * wh];

            for (int y = 0; y < wh; y++)
            {
                for (int x = 0; x < wh; x++)
                {
                    float t = 0;
                    float h = map[x, y];

                    Color upper = Color.white;
                    Color lower = Color.black;

                    colourmap[y * wh + x] = gradient.Evaluate((h + 1) / 2);
                }
            }

            texture.SetPixels(colourmap);
            texture.Apply();

            return texture;
        }


        public static HeightMap operator +(HeightMap a, HeightMap b)
        {
            if (a.Length != b.Length)
                throw new System.Exception("Length of a did not match Length of b. a.Length: " + a.Length + ". b.Length: " + b.Length);
            if (a.Width != b.Width)
                throw new System.Exception("Width of a did not match Width of b. a.Width: " + a.Width + ". b.Width: " + b.Width);

            float[,] map = new float[a.Length, a.Width];

            for (int i = 0; i < a.Length; i++)
            {
                for (int j = 0; j < a.Length; j++)
                {
                    map[i, j] = a[i, j] + b[i, j];
                }
            }

            return new HeightMap(map);
        }

        public static HeightMap operator -(HeightMap a, HeightMap b)
        {
            if (a.Length != b.Length)
                throw new System.Exception("Length of a did not match Length of b. a.Length: " + a.Length + ". b.Length: " + b.Length);
            if (a.Width != b.Width)
                throw new System.Exception("Width of a did not match Width of b. a.Width: " + a.Width + ". b.Width: " + b.Width);

            float[,] map = new float[a.Length, a.Width];

            for (int i = 0; i < a.Length; i++)
            {
                for (int j = 0; j < a.Length; j++)
                {
                    map[i, j] = a[i, j] - b[i, j];
                }
            }

            return new HeightMap(map);
        }

        /// <summary>
        /// Returns a new heightmap, with all values multiplied with mul.
        /// </summary>
        /// <param name="a">Heightmap</param>
        /// <param name="mul">Multiplier</param>
        /// <returns></returns>
        public static HeightMap operator *(HeightMap a, float mul)
        {
            if (a == null)
                throw new System.Exception("a is null.");

            float[,] map = new float[a.Length, a.Width];

            for (int i = 0; i < a.Length; i++)
            {
                for (int j = 0; j < a.Length; j++)
                {
                    map[i, j] = a[i, j] * mul;
                }
            }

            return new HeightMap(map);
        }

        public HeightMap Simplified(int skips)
        {
            float[,] retval = new float[(Width - 1) / skips + 1, (Width - 1) / skips + 1];

            for (int i = 0; i < retval.GetLength(0); i++)
            {
                for (int j = 0; j < retval.GetLength(0); j++)
                {
                    retval[i, j] = map[i * skips, j * skips];
                }
            }

            return new HeightMap(retval);
        }


        public Color[] AdvancedColormap(Gradient heightGradient, Gradient slopeGradient, Func<float, float> fun)
        {
            int wh = Width;

            Color[] colourmap = new Color[wh * wh];

            for (int y = 0; y < wh; y++)
            {
                for (int x = 0; x < wh; x++)
                {
                    float h = map[x, y];

                    if (x > 0 && y > 0 && x < Width - 1 && y < Width - 1)
                    {
                        Color c1 = heightGradient.Evaluate((h + 1) / 2);
                        Color c2 = slopeGradient.Evaluate((h + 1) / 2);

                        float slope = 0;
                        float comp = 0;

                        comp = Mathf.Abs(map[x, y] - map[x + 1, y]);
                        if (comp > slope)
                            slope = comp;
                        comp = Mathf.Abs(map[x, y] - map[x - 1, y]);
                        if (comp > slope)
                            slope = comp;
                        comp = Mathf.Abs(map[x, y] - map[x, y + 1]);
                        if (comp > slope)
                            slope = comp;
                        comp = Mathf.Abs(map[x, y] - map[x, y - 1]);
                        if (comp > slope)
                            slope = comp;

                        comp = Mathf.Abs(map[x, y] - map[x + 1, y + 1]);
                        if (comp > slope)
                            slope = comp;
                        comp = Mathf.Abs(map[x, y] - map[x + 1, y - 1]);
                        if (comp > slope)
                            slope = comp;
                        comp = Mathf.Abs(map[x, y] - map[x - 1, y + 1]);
                        if (comp > slope)
                            slope = comp;
                        comp = Mathf.Abs(map[x, y] - map[x - 1, y - 1]);
                        if (comp > slope)
                            slope = comp;

                        colourmap[y * wh + x] = Color.Lerp(c1, c2, fun(slope) * Width);
                    }
                    else
                    {
                        colourmap[y * wh + x] = heightGradient.Evaluate((h + 1) / 2);
                    }
                }
            }

            return colourmap;
        }

        public float[,] ToArray()
        {
            float[,] retval = new float[Width, Width];

            for (int j = 0; j < Width; j++)
            {
                for (int i = 0; i < Width; i++)
                {
                    retval[i, j] = map[i, j];
                }
            }

            return retval;
        }

        public Graph GetGraph(float maxHeightDifference, float differenceFactor, float minHeight, float maxHeight, float random, float baseWeight = 1)
        {
            Graph retval = new Graph();

            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    //if (map[i, j] < minHeight || map[i, j] > maxHeight)
                    //    continue;

                    retval.AddVertex((float)i / (float)Width, (float)j / (float)Width);
                    retval.AddVertex((float)i, (float)j);
                }
            }
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    if (map[i, j] < minHeight || map[i, j] > maxHeight)
                        continue;

                    float r = Generator.Value(map[0, 0], map[j, i]);

                    if (j < Width-1 && map[i, j + 1] >= minHeight && map[i, j + 1] <= maxHeight && Mathf.Abs(map[i, j] - map[i, j + 1]) < maxHeightDifference)
                        retval.AddEdge(retval.GetVertices()[j + i * Width], 
                                       retval.GetVertices()[(j + 1) + i * Width], 
                                       differenceFactor * Mathf.Abs(map[i, j] - map[i, j + 1]) * Mathf.Abs(map[i, j] - map[i, j + 1]) + 
                                       baseWeight + 
                                       r * random);

                    if (j > 0 && map[i, j - 1] >= minHeight && map[i, j - 1] <= maxHeight && Mathf.Abs(map[i, j] - map[i, j - 1]) < maxHeightDifference)
                        retval.AddEdge(retval.GetVertices()[j + i * Width], 
                                       retval.GetVertices()[(j - 1) + i * Width], 
                                       differenceFactor * Mathf.Abs(map[i, j] - map[i, j - 1]) * Mathf.Abs(map[i, j] - map[i, j - 1]) + 
                                       baseWeight +
                                       r * random);

                    if (i < Width - 1 && map[i + 1, j] >= minHeight && map[i + 1, j] <= maxHeight && Mathf.Abs(map[i, j] - map[i + 1, j]) < maxHeightDifference)
                        retval.AddEdge(retval.GetVertices()[j + i * Width], 
                                       retval.GetVertices()[j + (i + 1) * Width], 
                                       differenceFactor * Mathf.Abs(map[i, j] - map[i + 1, j]) * Mathf.Abs(map[i, j] - map[i + 1, j]) + 
                                       baseWeight +
                                       r * random);

                    if (i > 0 && map[i - 1, j] >= minHeight && map[i - 1, j] <= maxHeight && Mathf.Abs(map[i, j] - map[i - 1, j]) < maxHeightDifference)
                        retval.AddEdge(retval.GetVertices()[j + i * Width], 
                                       retval.GetVertices()[j + (i - 1) * Width], 
                                       differenceFactor * Mathf.Abs(map[i, j] - map[i - 1, j]) * Mathf.Abs(map[i, j] - map[i - 1, j]) + 
                                       baseWeight +
                                       r * random);
                }
            }

            return retval;
        }

        public float GetContinousHeight(float x, float y)
        {
            if (x > Width - 1 || x < 0 || y > Width - 1 || y < 0)
                throw new System.Exception("x and y must be between 0 and Length-1 (inclusive). x: " + x + ". y: " + y + ". Length: " + Width);

            int rx = Mathf.FloorToInt(x);
            int ry = Mathf.FloorToInt(y);

            if (rx == x && ry == y)
            {
                return map[rx, ry];
            } else if (rx == x)
            {
                return Mathf.Lerp(map[rx, ry], map[rx, ry+1], y.FractionalDigits());
            } else if (ry == y)
            {
                return Mathf.Lerp(map[rx, ry], map[rx + 1, ry], x.FractionalDigits());
            }
            float h0 = Mathf.Lerp(map[rx,ry], map[rx, ry+1], y.FractionalDigits());
            float h1 = Mathf.Lerp(map[rx+1, ry], map[rx+1, ry + 1], y.FractionalDigits());

            return Mathf.Lerp(h0, h1, x.FractionalDigits());

        }

        public float GetSlope(float x, float y)
        {
            if ((float)((int)y) != y && (float)((int)x) != x)
            {
                return GetSlope((int)x, (int)y);
            }

            float min = 2;
            float max = -2;

            if (map[(int)x, (int)y] > max)
                max = map[Mathf.FloorToInt(x), Mathf.FloorToInt(y)];

            if (map[(int)x, (int)y] < min)
                min = map[Mathf.FloorToInt(x), Mathf.FloorToInt(y)];

            if ((float)((int)x) != x)
            {
                if (map[(int)x + 1, (int)y] > max)
                    max = map[Mathf.RoundToInt(x), Mathf.RoundToInt(y)];
                if (map[(int)x + 1, (int)y] < min)
                    min = map[Mathf.RoundToInt(x), Mathf.RoundToInt(y)];
            }

            if ((float)((int)y) != y)
            {
                if (map[(int)x, (int)y + 1] > max)
                    max = map[Mathf.RoundToInt(x), Mathf.RoundToInt(y)];

                if (map[(int)x, (int)y + 1] < min)
                    min = map[Mathf.RoundToInt(x), Mathf.RoundToInt(y)];
            }

            if ((float)((int)y) != y && (float)((int)x) != x)
            {
                if (map[(int)x + 1, (int)y + 1] > max)
                    max = map[Mathf.RoundToInt(x), Mathf.RoundToInt(y)];

                if (map[(int)x + 1, (int)y + 1] < min)
                    min = map[Mathf.RoundToInt(x), Mathf.RoundToInt(y)];
            }

            return max - min;
        }

        public float GetSlope(int x, int y)
        {
            int width = Width;
            float slope = 0;
            float comp = 0;

            if (x > 0)
            {
                comp = Mathf.Abs(map[x, y] - map[x - 1, y]);
                if (comp > slope)
                    slope = comp;
            }

            if (x < width - 1)
            {
                comp = Mathf.Abs(map[x, y] - map[x + 1, y]);
                if (comp > slope)
                    slope = comp;
            }

            if (y > 0)
            {
                comp = Mathf.Abs(map[x, y] - map[x, y - 1]);
                if (comp > slope)
                    slope = comp;
            }

            if (y < width - 1)
            {
                comp = Mathf.Abs(map[x, y] - map[x, y + 1]);
                if (comp > slope)
                    slope = comp;
            }

            if (y < width - 1 && x < width - 1)
            {
                comp = Mathf.Abs(map[x, y] - map[x + 1, y + 1]);
                if (comp > slope)
                    slope = comp;
            }

            if (x > 0 && y > 0)
            {
                comp = Mathf.Abs(map[x, y] - map[x - 1, y - 1]);
                if (comp > slope)
                    slope = comp;
            }

            if (x < width - 1 && y > 0)
            {
                comp = Mathf.Abs(map[x, y] - map[x + 1, y - 1]);
                if (comp > slope)
                    slope = comp;
            }

            if (x > 0 && y < width - 1)
            {
                comp = Mathf.Abs(map[x, y] - map[x - 1, y + 1]);
                if (comp > slope)
                    slope = comp;
            }

            return slope * (width - 1);
        }

        public float[,] GetSlopeMap()
        {
            float[,] retval = new float[Width, Width];
            int width = Width;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < width; y++)
                {
                    float slope = 0;
                    float comp = 0;

                    if (x > 0)
                    {
                        comp = Mathf.Abs(map[x, y] - map[x - 1, y]);
                        if (comp > slope)
                            slope = comp;
                    }

                    if (x < width - 1)
                    {
                        comp = Mathf.Abs(map[x, y] - map[x + 1, y]);
                        if (comp > slope)
                            slope = comp;
                    }

                    if (y > 0)
                    {
                        comp = Mathf.Abs(map[x, y] - map[x, y - 1]);
                        if (comp > slope)
                            slope = comp;
                    }

                    if (y < width - 1)
                    {
                        comp = Mathf.Abs(map[x, y] - map[x, y + 1]);
                        if (comp > slope)
                            slope = comp;
                    }

                    if (y < width - 1 && x < width - 1)
                    {
                        comp = Mathf.Abs(map[x, y] - map[x + 1, y + 1]);
                        if (comp > slope)
                            slope = comp;
                    }

                    if (x > 0 && y > 0)
                    {
                        comp = Mathf.Abs(map[x, y] - map[x - 1, y - 1]);
                        if (comp > slope)
                            slope = comp;
                    }

                    if (x < width - 1 && y > 0)
                    {
                        comp = Mathf.Abs(map[x, y] - map[x + 1, y - 1]);
                        if (comp > slope)
                            slope = comp;
                    }

                    if (x > 0 && y < width - 1)
                    {
                        comp = Mathf.Abs(map[x, y] - map[x - 1, y + 1]);
                        if (comp > slope)
                            slope = comp;
                    }

                    retval[x, y] = slope * (width - 1);
                }
            }
            return retval;
        }
    }
}