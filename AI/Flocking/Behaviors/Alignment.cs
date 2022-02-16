using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.AI.Flocking
{
    [CreateAssetMenu(menuName = "Assets/Flock/Behavior/Alignment")]
    public class Alignment : FlockingBehavior
    {
        public float range;
        public override Vector2 VelocityChange(Flock flock, Boid boid, List<Transform> context)
        {
            Vector3 v = Vector2.zero;
            List<Boid> bb = flock.BoidsInRadius(boid.transform.position, range);
            foreach (Boid b in bb)
            {
                v += b.velocity;
            }
            return v / bb.Count;
        }
    }
}