using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.GraphCreator;
using JStuff.Utilities;

namespace JStuff.Generation.Terrain
{
    [CreateNodePath("Terrain/HeightMap/From Texture")]
    public class TextureToHeightMap : TerrainNode
    {
        public enum TextureType
        {
            RGB,
            Grayscale
        }

        // Member fields
        public Texture2D texture;
        public TextureType textureType = TextureType.Grayscale;

        public override bool IsConstant() => true;
        public override bool CacheOutput => false;

        protected override void SetupPorts()
        {
            if (textureType == TextureType.Grayscale)
            {
                AddOutputLink<HeightMap>(Evaluate_Grayscale);
            } else if (textureType == TextureType.RGB)
            {
                AddOutputLink<HeightMap>(Evaluate_R, portName: "HeightMap (red)");
                AddOutputLink<HeightMap>(Evaluate_G, portName: "HeightMap (green)");
                AddOutputLink<HeightMap>(Evaluate_B, portName: "HeightMap (blue)");
            }
        }

        HeightMap Evaluate_Grayscale()
        {
            if (texture == null)
                throw new System.Exception("Texture is null.");

            float[,] retval = new float[texture.height, texture.width];

            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    Color color = texture.GetPixel(x, y);
                    retval[x, y] = (color.r + color.g + color.b).Remap(0, 3, -1, 1);
                }
            }

            return new HeightMap(retval);
        }

        HeightMap Evaluate_R()
        {
            if (texture == null)
                throw new System.Exception("Texture is null.");

            float[,] retval = new float[texture.height, texture.width];

            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    Color color = texture.GetPixel(x, y);
                    retval[x, y] = color.r.Remap(0f, 1f, -1f, 1f);
                }
            }

            return new HeightMap(retval);
        }

        HeightMap Evaluate_G()
        {
            if (texture == null)
                throw new System.Exception("Texture is null.");

            float[,] retval = new float[texture.height, texture.width];

            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    Color color = texture.GetPixel(x, y);
                    retval[x, y] = color.g.Remap(0f, 1f, -1f, 1f);
                }
            }

            return new HeightMap(retval);
        }

        HeightMap Evaluate_B()
        {
            if (texture == null)
                throw new System.Exception("Texture is null.");

            float[,] retval = new float[texture.height, texture.width];

            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    Color color = texture.GetPixel(x, y);
                    retval[x, y] = color.b.Remap(0f, 1f, -1f, 1f);
                }
            }

            return new HeightMap(retval);
        }

        public override Node Clone()
        {
            TextureToHeightMap retval = base.Clone() as TextureToHeightMap;

            retval.texture = texture;
            retval.textureType = textureType;

            return retval;
        }
    }
}