using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.GraphCreator;

namespace JStuff.Generation.Terrain
{
    [CreateNodePath("Terrain/Points/New/Grid Placement")]
    public class GridPlacement : TerrainNode
    {
        public float density;
        [Range(0f,1f)]
        public float randomness;

        InputLink<int> seedInput;

        InputLink<float> chunkSizePropertyInput;

        OutputLink<List<Vector2>> output;

        protected override void SetupPorts()
        {
            seedInput = AddInputLink<int>();

            chunkSizePropertyInput = AddPropertyInputLink<float>("chunkSize");

            output = AddOutputLink<List<Vector2>>(Evaluate);
        }

        List<Vector2> Evaluate()
        {
            System.Random rng = new System.Random(seedInput.Evaluate());

            float chunkSize = chunkSizePropertyInput.Evaluate() - 1;
            int points = (int)((chunkSize) * density); // -1???
            float distance = chunkSize / points;
            float offset = distance / 2f;

            List<Vector2> retval = new List<Vector2>();

            for (int y = 0; y < points; y++)
            {
                for (int x = 0; x < points; x++)
                {
                    retval.Add(new Vector2(
                        Mathf.Clamp(x * distance + offset + (float)rng.NextDouble() * randomness * offset, 0, chunkSize), 
                        Mathf.Clamp(y * distance + offset + (float)rng.NextDouble() * randomness * offset, 0, chunkSize)
                        )
                    );
                    Vector2 p = retval[retval.Count - 1];
                    if (p.x > chunkSize + 1 || p.y > chunkSize + 1)
                    {
                        Debug.Log("Wrong point!: " + p);
                    }
                }
            }

            return retval;
        }

        public override Node Clone()
        {
            GridPlacement retval = base.Clone() as GridPlacement;

            retval.density = density;
            retval.randomness = randomness;

            return retval;
        }
    }
}