using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.Collections
{
    [System.Serializable]
    public struct WeightedPrefab: IWeightedPrefab
    {
        public GameObject prefab;
        [Range(0.0f, 1f)]
        public float weight;

        public WeightedPrefab(GameObject prefab, float weight)
        {
            this.prefab = prefab;
            this.weight = weight;
        }

        public GameObject Prefab => prefab;

        public float Weight => weight;
    }
}