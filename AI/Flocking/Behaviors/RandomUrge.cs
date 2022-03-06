using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.AI.Flocking
{
    [CreateAssetMenu(menuName = "JStuff/Flock/Behavior/Random")]
    public class RandomUrge : FlockingBehavior
    {
        public override Vector2 VelocityChange(Flock flock, Boid boid, List<Transform> context)
        {
            float random = UnityEngine.Random.Range(0f, 260f);
            return new Vector2(Mathf.Cos(random), Mathf.Sin(random)).normalized;
        }
    }
}