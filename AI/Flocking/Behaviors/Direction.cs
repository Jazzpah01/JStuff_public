using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.AI.Flocking
{
    [CreateAssetMenu(menuName = "JStuff/Flock/Behavior/Direction")]
    public class Direction : FlockingBehavior
    {
        public override Vector2 VelocityChange(Flock flock, Boid boid, List<Transform> context)
        {
            return boid.velocity.normalized;
        }
    }
}