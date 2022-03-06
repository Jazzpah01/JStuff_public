using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.AI.Flocking
{
    [CreateAssetMenu(menuName = "JStuff/Flock/Behavior/Target")]
    public class Target : FlockingBehavior
    {
        public Vector2 targetOffset;
        public float range;
        public override Vector2 VelocityChange(Flock flock, Boid boid, List<Transform> context)
        {
            if (context == null || context.Count == 0)
                return Vector2.zero;
            Vector2 retval = Vector2.zero;
            Transform target = null;
            float distance = range;
            foreach(Transform potential in context)
            {
                Vector2 v = potential.transform.position;
                Vector2 v2 = boid.transform.position;
                if ((v - v2).magnitude < distance)
                    target = potential;
            }
            if (target == null)
                return Vector2.zero;
            return ((Vector2)target.position - (Vector2)boid.transform.position + targetOffset).normalized;
        }
    }
}