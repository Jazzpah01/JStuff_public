using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.AI.Flocking
{
    [System.Serializable]
    public abstract class FlockingBehavior : ScriptableObject
    {
        public abstract Vector2 VelocityChange(Flock flock, Boid boid, List<Transform> context);
    }
}