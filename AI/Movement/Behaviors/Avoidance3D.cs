using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.Utilities;
using JStuff.VectorSwizzling;

namespace JStuff.AI.Steering
{
    [CreateAssetMenu(menuName ="JStuff/AI/Steering/Avoidance 3D")]
    public class Avoidance3D : SteeringSO
    {
        [Min(0.1f)]
        public float avoidDistance = 1;

        public override SteeringOutput GetSteering(ISteeringAgent agent)
        {
            Vector2 position = agent.Position;

            Collider[] hits = Physics.OverlapSphere(agent.transform.position, agent.Radius + avoidDistance);

            Vector2 targetVelocity = Vector2.zero;

            foreach (Collider hit in hits)
            {
                if (hit.transform == agent.CollisionTransform)
                    continue;

                Vector2 closestPoint = hit.ClosestPoint(agent.CollisionTransform.position);
                float distance = (position - closestPoint).magnitude;

                targetVelocity += (position - closestPoint).normalized * distance.Remap(agent.Radius, avoidDistance, 1, 0).Clamp(0,1);
            }

            return new SteeringOutput(targetVelocity * agent.MaxAcceleration);
        }
    }
}