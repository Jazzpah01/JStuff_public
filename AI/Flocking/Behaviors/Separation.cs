using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.AI.Flocking
{
    [CreateAssetMenu(menuName = "JStuff/Flock/Behavior/Separation")]
    public class Separation : FlockingBehavior
    {
        public float distance;
        public override Vector2 VelocityChange(Flock flock, Boid boid, List<Transform> context)
        {
            Transform trans = boid.transform;
            float dist = float.MaxValue;

            if (context == null || context.Count == 0)
                return new Vector2(0, 0);

            foreach (Transform t in context)
            {
                if (t == boid.transform)
                    continue;

                if ((boid.transform.position - t.position).magnitude < dist)
                {
                    dist = (boid.transform.position - t.position).magnitude;
                    trans = t;
                }
            }

            Vector2 v = (Vector2)trans.position - (Vector2)boid.transform.position;
            float r = 1 - 2 * distance / (v.magnitude + distance);
            return (dist != 0) ? r * v.normalized : r * new Vector2(Random.Range(-1, 1), Random.Range(-1, 1)).normalized;
        }
    }
}


/*
namespace JStuff.AI.Flocking
{
    [CreateAssetMenu(menuName = "Assets/Flock/Behavior/Separation")]
    public class Separation : FlockingBehavior
    {
        public float distance;
        public override Vector2 VelocityChange(Flock flock, Boid boid, List<Transform> context)
        {
            Transform trans = boid.transform;
            float dist = float.MaxValue;

            if (context == null || context.Count == 0)
                throw new System.Exception("Can't have empty context");

            foreach (Transform t in context)
            {
                if (t == boid.transform)
                    continue;

                if ((boid.transform.position - t.position).magnitude < dist)
                {
                    dist = (boid.transform.position - t.position).magnitude;
                    trans = t;
                }
            }

            Vector2 v = (Vector2)trans.position - (Vector2)boid.transform.position;
            float r = 1 - 2 * distance / (v.magnitude + distance);
            return (dis != 0) ? r * v.normalized : r * new Vector2(Random.Range(-1, 1), Random.Range(-1, 1)).normalized;
        }
    }
}




    ------------------------------------
   namespace JStuff.AI.Flocking
{
    [CreateAssetMenu(menuName = "Assets/Flock/Behavior/Separation")]
    public class Separation : FlockingBehavior
    {
        public float distance;
        public override Vector2 VelocityChange(Flock flock, Boid boid, List<Transform> context)
        {
            (Boid b, float dis) = flock.NearestBoid(boid);
            Vector2 v = b.transform.position - boid.transform.position;
            float r = 1 - 2 * distance / (v.magnitude + distance);
            return (dis != 0) ? r * v.normalized : r * new Vector2(Random.Range(-1, 1), Random.Range(-1, 1)).normalized;
        }
    }
}
*/
