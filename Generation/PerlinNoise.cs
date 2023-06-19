using UnityEngine;
using System.Collections;
using JStuff.Generation;

//https://github.com/SebLague/Procedural-Landmass-Generation
public static class PerlinNoise
{

	public enum NormalizeMode { Local, Global };

	public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
	{
		float[,] noiseMap = new float[mapWidth, mapHeight];

		System.Random prng = new System.Random(seed);
		Vector2[] octaveOffsets = new Vector2[octaves];

		float maxPossibleHeight = 0;
		float amplitude = 1;
		float frequency = 1;

		for (int i = 0; i < octaves; i++)
		{
			float offsetX = prng.Next(-100000, 100000) + offset.x;
			float offsetY = prng.Next(-100000, 100000) + offset.y;
			octaveOffsets[i] = new Vector2(offsetX, offsetY);

			maxPossibleHeight += amplitude;
			amplitude *= persistance;
		}

		if (scale <= 0)
		{
			scale = 0.0001f;
		}

		for (int y = 0; y < mapHeight; y++)
		{
			for (int x = 0; x < mapWidth; x++)
			{

				amplitude = 1;
				frequency = 1;
				float noiseHeight = 0;

				for (int i = 0; i < octaves; i++)
				{
					float sampleX = ((float)x / (mapHeight - 1) + octaveOffsets[i].x) / scale * frequency;
					float sampleY = ((float)y / (mapHeight - 1) + octaveOffsets[i].y) / scale * frequency;

					float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
					noiseHeight += perlinValue * amplitude;

					amplitude *= persistance;
					frequency *= lacunarity;
				}

				noiseMap[x, y] = noiseHeight;
			}
		}

		return noiseMap;
	}


	public static HeightMap GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, Vector2 offset)
	{
		//mapWidth++;
		//mapHeight++;
		float[,] noiseMap = new float[mapWidth, mapHeight];

		(float s0, float s1) = Noise.GetNormalSeeds(seed);

		if (scale <= 0)
		{
			scale = 0.0001f;
		}

		for (int y = 0; y < mapHeight; y++)
		{
			for (int x = 0; x < mapWidth; x++)
			{
                //float sampleX = x / scale + offset.x;
                //float sampleY = y / scale + offset.y;

                //float sampleX = (x + offset.x) / scale + offset.x;
                //float sampleY = (y + offset.y) / scale + offset.y;

                float sampleX = ((float)x / (mapWidth-1) + offset.x) / scale;
                float sampleY = ((float)y / (mapWidth-1) + offset.y) / scale;

                noiseMap[x, y] = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
			}
		}

		return new HeightMap(noiseMap);
	}
}