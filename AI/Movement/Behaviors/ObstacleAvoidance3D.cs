using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.Utilities;
using JStuff.VectorSwizzling;

namespace JStuff.AI.Steering
{
    [CreateAssetMenu(menuName ="JStuff/AI/Steering/Obstacle Avoidance 3D")]
    public class ObstacleAvoidance3D : SteeringSO
    {

        [Min(0)]
        public float avoidDistance = 0;
        [Min(0.01f)]
        public float lookahead = 0.5f;
        [Min(0)]
        public float moveAway = 0;
        public LayerMask layerMask;

        private Seek seek;

        private void OnEnable()
        {
            seek = CreateInstance<Seek>();
        }

        public override SteeringOutput GetSteering(ISteeringAgent thisAgent)
        {
            Vector2 dir2d = thisAgent.Velocity.normalized;
            Vector3 direction = new Vector3(dir2d.x, 0, dir2d.y);

            //RaycastHit2D[] hits =
            //    Physics2D.RaycastAll(thisAgent.transform.position, direction, lookahead);

            RaycastHit[] hits =
                Physics.SphereCastAll(thisAgent.transform.position, thisAgent.Radius, direction, lookahead, layerMask);

            if (hits.Length == 0)
                return new SteeringOutput();

            RaycastHit ahit = hits[0];
            bool hasHit = false;
            float d = float.MaxValue;
            foreach (RaycastHit hit in hits)
            {
                if (hit.transform == thisAgent.CollisionTransform)
                    continue;

                if (hit.distance > d)
                    continue;

                d = hit.distance;

                ahit = hit;

                hasHit = true;
            }

            if (!hasHit)
                return new SteeringOutput();

            Vector2 targetPosition = ahit.point.xz() + ahit.normal.xz().normalized * (avoidDistance + thisAgent.Radius);

            if (d - thisAgent.Radius < avoidDistance)
            {
                Vector2 newDirection = thisAgent.Position - targetPosition;
                targetPosition += newDirection.normalized * moveAway * (avoidDistance / Mathf.Min((d - thisAgent.Radius), 0.00001f));
            }

            //GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //go.transform.position = new Vector3(targetPosition.x, 0, targetPosition.y);

            return seek.GetSteering(thisAgent, targetPosition);
        }
    }
}