using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.Randomness;
using System;
using JStuff.Generation;

namespace JStuff.Generation
{
    [CreateAssetMenu(menuName = "Subclimate")]
    public class Subclimate : ScriptableObject, IClimate
    {
        // Temp represented as 0 to 1, where 0 is -40 and 1 is 40

        public const float universalMaxSlope = 33f;

        public Gradient sedimentsOnHeight;
        public InterpolateFunction foliageOnTemp;
        public InterpolateFunction foliageOnHeight;
        public InterpolateFunction tempOnHeight;
        public InterpolateFunction snowOnTemp;
        public InterpolateFunction slope;
        public InterpolateFunction grassOnFoliage;

        public InterpolateFunction grassOnGrass;

        public float greenFactor = 1f;

        public float minHeightForGrass = 0.001f;
        public float maxGrassOnHeight = 0.8f;
        public float grassRandomness = 0f;

        public float grassLikelihood = 1;
        public int grassprSquareM = 100;
        public Material grassMaterial;
        public int grassOnDistance = 1;
        public float grassRadious = 1;

        public Foliage[] foliage;

        bool firstcall = true;

        public void Initialize()
        {
            foliageOnTemp.PreEvaluate();
            foliageOnHeight.PreEvaluate();
            tempOnHeight.PreEvaluate();
            snowOnTemp.PreEvaluate();
            slope.PreEvaluate();
            grassOnFoliage.PreEvaluate();
            grassOnGrass.PreEvaluate();
        }

        public Color[] GetColormap(HeightMap map, HeightMap greenMap, float heightmapStretch)
        {
            int width = map.Width;
            Color[] colourmap = new Color[width * width];

            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float h = (map[x, y] + 1) / 2;

                    float gx = ((float)x) / (float)map.Width * (float)greenMap.Width;
                    float gy = ((float)y) / (float)map.Width * (float)greenMap.Width;

                    float temp = tempOnHeight.Evaluate(h);

                    float g = greenMap[(int)gx, (int)gy];
                    g = GetFoliage(g, h);

                    g = grassOnFoliage.Evaluate(g);

                    float r = sedimentsOnHeight.Evaluate(h).r;

                    float b = slope.Evaluate(GetSlope(map, x, y)) * heightmapStretch * universalMaxSlope;

                    float a = snowOnTemp.Evaluate(temp);


                    if (g > minHeightForGrass && g < maxGrassOnHeight)
                    {
                        g = g.Remap(minHeightForGrass, maxGrassOnHeight, 0, 1);
                    }
                    else if (g >= minHeightForGrass)
                    {
                        g = 1;
                    }
                    else
                    {
                        g = 0;
                    }

                    colourmap[x + y * width] = new Color(
                        r.Clamp(0, 1),
                        g,//(g + g * grassRandomness * RandomGenerator.Value(g, Mathf.Abs(h))).Clamp(0, 1),
                        b.Clamp(0, 1),
                        a.Clamp(0, 1));
                }
            }


            return colourmap;
        }

        public List<Vector2> FilterGrassPoints(List<Vector2> points, Color[] colormap, int colormapWidth)
        {
            if (points.Count < 1)
                throw new System.Exception("Can't filter an empty list.");

            float seed1 = colormap[0].g;
            float seed2 = colormap[colormap.Length-1].g;
            float seed3 = RandomGenerator.NormalValue(seed1, seed2);

            List<Vector2> retval = new List<Vector2>();

            foreach (Vector2 p in points)
            {
                int x = Mathf.FloorToInt(p.x * colormapWidth);
                int y = Mathf.FloorToInt(p.y * colormapWidth);

                if (grassOnGrass.Evaluate(colormap[x + y * colormapWidth].g - colormap[x + y * colormapWidth].b) >= RandomGenerator.NormalValue(seed1, seed2) / grassLikelihood)
                {
                    retval.Add(p);
                }

                seed1 = RandomGenerator.NormalValue(seed2, seed3);
                seed2 = RandomGenerator.NormalValue(seed1, seed3);
                seed3 = RandomGenerator.NormalValue(seed1, seed2);
            }

            return retval;
        }

        public List<Vector2> FilterFoliage(List<Vector2> points, float size, HeightMap heightmap, HeightMap greenmap, float heightmapStretch, int foliageIndex)
        {
            List<Vector2> retval = new List<Vector2>();

            for (int i = 0; i < points.Count; i++)
            {
                // Get heightmap greenmap coordinate
                int gx = Mathf.FloorToInt(points[i].x / size * greenmap.Width);
                int gz = Mathf.FloorToInt(points[i].y / size * greenmap.Width);

                int hx = Mathf.FloorToInt(points[i].x / size * heightmap.Width);
                int hz = Mathf.FloorToInt(points[i].y / size * heightmap.Width);

                float height = heightmap[hx, hz];
                float green = greenmap[gx, gz];
                green = GetFoliage(green, height);

                // Check height of heightmap
                if (height < foliage[foliageIndex].minHeight)
                    continue;
                if (height > foliage[foliageIndex].maxHeight)
                    continue;

                // Multiply by greenmap and randomness
                if (green < foliage[foliageIndex].minGreenHeight)
                    continue;
                if (green > foliage[foliageIndex].maxGreenHeight)
                    continue;

                if (foliage[foliageIndex].maxSlope < GetSlope(heightmap, hx, hz) * universalMaxSlope * heightmapStretch)
                {
                    continue;
                }

                // Note: Size of heightmap can wary, so only the first value will be used to ensure consistent foliage.
                // Size of greenmap is always the same.
                if (green < foliage[foliageIndex].optimalGreenHeight)
                {
                    if (green.Remap(foliage[foliageIndex].minGreenHeight, foliage[foliageIndex].optimalGreenHeight, 0, 1) >
                        RandomGenerator.NormalValue(green, heightmap[0, 0]))
                    {
                        retval.Add(points[i]);
                    }
                }
                else
                {
                    if (green.Remap(foliage[foliageIndex].minGreenHeight, foliage[foliageIndex].optimalGreenHeight, 0, 1) >
                        RandomGenerator.NormalValue(green, heightmap[0, 0]))
                    {
                        retval.Add(points[i]);
                    }
                }
            }

            return retval;
        }

        private float GetFoliage(float foliage, float height)
        {
            float temp = tempOnHeight.Evaluate(height);
            float g = foliage;

            g *= foliageOnTemp.Evaluate(temp);

            g *= foliageOnHeight.Evaluate((height+1)/2);

            return g.Clamp(0, 1);
        }








        private float GetSlope(HeightMap map, int x, int y)
        {
            int width = map.Width;
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

        private float[,] GetSlopeMap(HeightMap map)
        {
            float[,] retval = new float[map.Width, map.Width];
            int width = map.Width;
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

                    retval[x, y] = slope;
                }
            }
            return retval;
        }
    }
}