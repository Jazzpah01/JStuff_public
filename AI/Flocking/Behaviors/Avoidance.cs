using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.AI.Flocking
{
    [CreateAssetMenu(menuName = "JStuff/Flock/Behavior/Avoidance")]
    public class Avoidance : FlockingBehavior
    {
        public float range;
        public override Vector2 VelocityChange(Flock flock, Boid boid, List<Transform> context)
        {
            Vector2 retval = Vector2.zero;
            foreach (Transform t in context)
            {
                if (t == boid.transform)
                    continue;

                Vector2 delta = t.position - boid.transform.position;
                if ((delta).magnitude < range)
                {
                    retval -= delta.normalized * range/delta.magnitude;
                }
            }
            return retval;
        }
    }
}