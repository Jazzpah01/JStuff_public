using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.GraphCreator;
using JetBrains.Annotations;

namespace JStuff.Generation.Terrain
{
    [CreateNodePath("Terrain/HeightMap/Layer Mask")]
    public class HeightMapLayerMask : TerrainNode
    {
        InputLink<HeightMap> layer1;
        InputLink<HeightMap> layer2;
        InputLink<HeightMap> layerMask;

        protected override void SetupPorts()
        {
            layer1 = AddInputLink<HeightMap>();
            layer2 = AddInputLink<HeightMap>();
            layerMask = AddInputLink<HeightMap>();
            AddOutputLink<HeightMap>(Evaluate);
        }

        HeightMap Evaluate()
        {
            HeightMap l1HM = layer1.Evaluate();
            HeightMap l2HM = layer2.Evaluate();
            HeightMap lMaskHM = layerMask.Evaluate();

            if (l1HM.Width != l2HM.Width || l2HM.Width != lMaskHM.Width)
                throw new System.Exception("Width of heightmap layers is not identical.");

            float[,] retval = new float[l1HM.Width, l1HM.Length];

            for (int y = 0; y < l1HM.Length; y++)
            {
                for (int x = 0; x < l1HM.Width; x++)
                {
                    float t = (lMaskHM[x, y] + 1) / 2;

                    retval[x,y] = Mathf.Lerp(l1HM[x, y], l2HM[x, y], t);
                }
            }

            return new HeightMap(retval);
        }
    }
}