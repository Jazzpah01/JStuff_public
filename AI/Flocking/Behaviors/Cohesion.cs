using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.AI.Flocking
{
    [CreateAssetMenu(menuName = "JStuff/Flock/Behavior/Cohesion")]
    public class Cohesion : FlockingBehavior
    {
        public float range;
        public override Vector2 VelocityChange(Flock flock, Boid boid, List<Transform> context)
        {
            Vector2 v = new Vector2(0, 0);
            List<Transform> inRange = new List<Transform>();

            foreach (Transform t in context)
            {
                if ((t.position - boid.transform.position).magnitude > range)
                {
                    inRange.Add(t);
                }
            }

            foreach (Transform t in inRange)
            {
                Vector2 v2 = t.transform.position;
                v += v2;
            }
            v /= inRange.Count;
            Vector2 v3 = boid.transform.position;
            v -= v3;
            return (v.magnitude > 0) ? v.normalized : new Vector2(0, 0);
        }
    }
}