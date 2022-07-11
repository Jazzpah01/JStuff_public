using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.AI.Steering
{
    public abstract class SteeringSO : ScriptableObject, ISteeringBehavior
    {
        public abstract SteeringOutput GetSteering(ISteeringAgent agent);
    }
}