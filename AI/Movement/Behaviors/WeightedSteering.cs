using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.AI.Steering
{
    [System.Serializable]
    public class WeightedSteering
    {
        public SteeringSO behavior;
        public float weight;
    }
}