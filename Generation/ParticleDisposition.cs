using JStuff.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.Generation
{
    /// <summary>
    /// Doesn't work right now.
    /// </summary>
    public static class ParticleDisposition
    {
        public delegate int[,] DropStrategy(int[,] map, int dispHight, int particleHight, int amount, int maxHeight,
        System.Func<int[,], int, int, (int, int, int)> inc,
        System.Func<int[,], int, int, (int, int)[]> unba,
        System.Func<int[,], int, int, (int, int)> brownmove);

        public delegate int[,] DropStrategyAlternative<T>(int[,] map, int dispHight, int particleHight, int maxHeight,
            System.Func<int[,], int, int, (int, int, int)> inc,
            System.Func<int[,], int, int, (int, int)[]> unba,
            System.Func<int[,], int, int, (int, int)> brownmove,
            T context);

        public static int[,] Generate(int size, int dispHight, int particleHight, int amount, int maxHeight,
            DropStrategy strategy)
        {
            int[,] retval = new int[size, size];

            retval = GenerateHelper(retval, dispHight, particleHight, amount, maxHeight, strategy);

            return retval;
        }

        public static int[,] Generate(int[,] map, int dispHight, int particleHight, int amount, int maxHeight,
            DropStrategy strategy)
        {
            int[,] retval = map.Copy();

            retval = GenerateHelper(retval, dispHight, particleHight, amount, maxHeight, strategy);

            return retval;
        }

        private static int[,] GenerateHelper(int[,] map, int dispHight, int particleHight, int amount, int maxHeight,
            DropStrategy strategy)
        {
            //int giveUpMax = 10;
            //int giveUp = giveUpMax;
            //
            //
            //
            //
            //
            //
            //
            //map = strategy(map, dispHight, particleHight, amount, maxHeight,
            //    Increase, UnbalancedNeightbors, BrownianMovement);
            //
            //return map;
            throw new System.Exception();
        }

        public static (int, int, int) Increase(int[,] nmap, int x, int y)
        {
            //(int, int)[] nightbors = UnbalancedNeightbors(nmap, x, y);
            //int i = Mathf.Min(maxHeight - nmap[x, y], particleHight);
            //if (nightbors == null)
            //{
            //    if (i == 0)
            //    {
            //        giveUp--;
            //        if (giveUp <= 0)
            //        {
            //            Debug.Log("Particle Disposition Increase failed to find a suitible spot for drop.");
            //            return (x, y, i);
            //        }
            //        (int nx, int ny) = BrownianMovement(nmap, x, y);
            //        return Increase(nmap, nx, ny);
            //    }
            //    return (x, y, i);
            //}
            //int index = Random.Range(0, nightbors.Length - 1);
            //giveUp = giveUpMax;
            //return Increase(nmap, nightbors[index].Item1, nightbors[index].Item2);
            //
            throw new System.Exception();
        }

        public static (int, int)[] UnbalancedNeightbors(int[,] nmap, int x, int y, int dispHight)
        {
            List<(int, int)> retval = new List<(int, int)>();
            if (x > 0 && nmap[x, y] >= nmap[x - 1, y] + dispHight)
                retval.Add((x - 1, y));
            if (y > 0 && nmap[x, y] >= nmap[x, y - 1] + dispHight)
                retval.Add((x, y - 1));
            if (x < nmap.GetLength(0) - 1 && nmap[x, y] >= nmap[x + 1, y] + dispHight)
                retval.Add((x + 1, y));
            if (y < nmap.GetLength(1) - 1 && nmap[x, y] >= nmap[x, y + 1] + dispHight)
                retval.Add((x, y + 1));
            return (retval.Count > 0) ? retval.ToArray() : null;

            throw new System.Exception();
        }

        public static (int, int) BrownianMovement(int[,] nmap, int x, int y)
        {
            bool found = false;
            while (!found)
            {
                switch (UnityEngine.Random.Range(0, 4))
                {
                    case 0:
                        if (x < nmap.GetLength(0) - 1)
                            return (x + 1, y);
                        break;
                    case 1:
                        if (y < nmap.GetLength(1) - 1)
                            return (x, y + 1);
                        break;
                    case 2:
                        if (x > 0)
                            return (x - 1, y);
                        break;
                    case 3:
                        if (y > 0)
                            return (x, y - 1);
                        break;
                    default:
                        throw new System.Exception("Outside range");
                }
            }
            throw new System.Exception("Oof");
        }

        // Strategy for movement
        public static int[,] NormalStrategy(int[,] map, int dispHight, int particleHight, int amount, int maxHeight,
        System.Func<int[,], int, int, (int, int, int)> increase,
        System.Func<int[,], int, int, (int, int)[]> unbalancedNeightbors,
        System.Func<int[,], int, int, (int, int)> brownianMovement)
        {
            int size = map.GetLength(0);

            (int x, int y) point = (UnityEngine.Random.Range(0, size), UnityEngine.Random.Range(0, size));
            for (int i = 0; i < amount; i++)
            {
                (int incx, int incy, int inc) = increase(map, point.x, point.y);
                map[incx, incy] += inc;
                point = brownianMovement(map, point.x, point.y);
            }

            return map;
        }

        public static int[,] RandomStrategy(int[,] map, int dispHight, int particleHight, int amount, int maxHeight,
        System.Func<int[,], int, int, (int, int, int)> increase,
        System.Func<int[,], int, int, (int, int)[]> unbalancedNeightbors,
        System.Func<int[,], int, int, (int, int)> brownianMovement)
        {
            int size = map.GetLength(0);

            (int x, int y) point = (UnityEngine.Random.Range(0, size), UnityEngine.Random.Range(0, size));
            for (int i = 0; i < amount; i++)
            {
                (int incx, int incy, int inc) = increase(map, point.x, point.y);
                map[incx, incy] += inc;
                point = (UnityEngine.Random.Range(0, size), UnityEngine.Random.Range(0, size));
            }

            return map;
        }
    }
}