using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.GraphCreator;

namespace JStuff.Generation.Terrain
{
    [CreateNodePath("Terrain/HeightMap/Local Erosion")]
    public class LocalErosion : TerrainNode
    {
        public int iterations = 5;
        public float factor = 0.5f;
        [Range(-1f,1f)]public float bias = 0f;
        public bool randomPoints = false;

        InputLink<HeightMap> input;
        OutputLink<HeightMap> output;

        public override bool CacheOutput => true;

        protected override void SetupPorts()
        {
            input = AddInputLink<HeightMap>();
            output = AddOutputLink<HeightMap>(Evaluate);
        }

        HeightMap Evaluate()
        {
            if (randomPoints)
            {
                return Evaluate2();
            } else
            {
                return Evaluate1();
            }
        }

        HeightMap Evaluate1()
        {
            float[,] retval = input.Evaluate().ToArray();
            float[,] buffer = new float[retval.GetLength(0), retval.GetLength(1)];
            int size = retval.GetLength(0);

            System.Random rng = new System.Random(30);

            for (int c = 0; c < iterations; c++)
            {
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        buffer[x, y] = NewHeight(x, y, retval, size, rng);
                    }
                }

                // Swap buffer and retval
                float[,] temp = retval;
                retval = buffer;
                buffer = temp;
            }

            return new HeightMap(retval);
        }

        HeightMap Evaluate2()
        {
            float[,] retval = input.Evaluate().ToArray();
            int size = retval.GetLength(0);

            System.Random rng = new System.Random(30);

            int ri = iteration * size * size;

            for (int c = 0; c < ri; c++)
            {
                int x = rng.Next(size - 1);
                int y = rng.Next(size - 1);

                retval[x, y] = NewHeight(x, y, retval, size, rng);
            }

            return new HeightMap(retval);
        }

        private float NewHeight(int x, int y, float[,] retval, int size, System.Random rng)
        {
            //if (x == 0 || y == 0 || x == size - 1 || y == size - 1)
            //{
            //    return retval[x,y];
            //}
            if (x == 0 && y == 0 || x == size - 1 && y == size - 1 || x == 0 && y == size - 1 || x == size - 1 && y == 0)
            {
                return retval[x, y];
            }

            float mid = 0;
            float val = 0;
            float max = float.MinValue;
            float min = float.MaxValue;

            float f = 0.01f;

            if (x == 0 && y != 0 && y != size - 1)
            {
                max = Mathf.Max(retval[x, y - 1], retval[x, y + 1]);
                min = Mathf.Min(retval[x, y - 1], retval[x, y + 1]);
                mid = (max - min) / 2;
                val = retval[x, y];
                //return retval[x, y] + bias * f;
            }
            else if (x == size - 1 && y != 0 && y != size - 1)
            {
                max = Mathf.Max(retval[x, y - 1], retval[x, y + 1]);
                min = Mathf.Min(retval[x, y - 1], retval[x, y + 1]);
                mid = (max - min) / 2;
                val = retval[x, y];
                //return retval[x, y] + bias * f;
            }
            else if (y == 0 && x != 0 && x != size - 1)
            {
                max = Mathf.Max(retval[x - 1, y], retval[x + 1, y]);
                min = Mathf.Min(retval[x - 1, y], retval[x + 1, y]);
                mid = (max - min) / 2;
                val = retval[x, y];
                //return retval[x, y] + bias * f;
            }
            else if (y == size - 1 && x != 0 && x != size - 1)
            {
                max = Mathf.Max(retval[x - 1, y], retval[x + 1, y]);
                min = Mathf.Min(retval[x - 1, y], retval[x + 1, y]);
                mid = (max - min) / 2;
                val = retval[x, y];
                //return retval[x, y] + bias * f;
            }
            else
            {
                // Not on edge
                for (int i = -1; i < 2; i++)
                {
                    for (int j = -1; j < 2; j++)
                    {
                        if (i == 0 && j == 0)
                            continue;

                        if (retval[x + j, y + i] > max)
                        {
                            max = retval[x + j, y + i];
                        }
                        if (retval[x + j, y + i] < min)
                        {
                            min = retval[x + j, y + i];
                        }
                    }
                }

                mid = (max + min) / 2;
                val = retval[x, y];
            }

            //float r = Mathf.Abs((float)rng.NextDouble() % 1f);
            float r = Noise.NormalValue(max, min);

            if (min < val && val < max)
            {
                float aim = min + Mathf.Abs(max - min) * (0.5f + bias / 2 * (max - min));
                //float aim = min + Mathf.Abs(max - min) * (0.5f + bias / 2);

                if (val > aim)
                {
                    val -= Mathf.Abs(val - aim) * r * factor;
                }
                else if (val < aim)
                {
                    val += Mathf.Abs(val - aim) * r * factor;
                }
            }
            else if (min > val)
            {
                val += Mathf.Abs(val - max) * r * factor;
            }
            else if (max < val)
            {
                val -= Mathf.Abs(val - min) * r * factor;
            }

            return val;
        }

        public override Node Clone()
        {
            LocalErosion retval = base.Clone() as LocalErosion;
            retval.iterations = iterations;
            retval.bias = bias;
            retval.randomPoints = randomPoints;
            retval.factor = factor;
            return retval;
        }
    }
}