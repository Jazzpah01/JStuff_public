using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.Utilities;
using JStuff.VectorSwizzling;

namespace JStuff.AI.Steering
{
    [CreateAssetMenu(menuName = "JStuff/AI/Steering/Obstacle Avoidance 2D")]
    public class ObstacleAvoidance2D : SteeringSO
    {
        [Min(0)]
        public float avoidDistance = 0;
        [Min(0.01f)]
        public float lookahead = 0.5f;

        private Seek seek;

        private void OnEnable()
        {
            seek = CreateInstance<Seek>();
        }

        public override SteeringOutput GetSteering(ISteeringAgent thisAgent)
        {
            Vector2 direction = thisAgent.Velocity.normalized;

            //RaycastHit2D[] hits =
            //    Physics2D.RaycastAll(thisAgent.transform.position, direction, lookahead);

            RaycastHit2D[] hits =
                Physics2D.CircleCastAll(thisAgent.transform.position, thisAgent.Radius, direction, lookahead);

            if (hits.Length == 0)
                return new SteeringOutput();

            RaycastHit2D ahit = hits[0];
            bool hasHit = false;
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.transform == thisAgent.CollisionTransform)
                    continue;

                ahit = hit;

                hasHit = true;
            }

            if (!hasHit)
                return new SteeringOutput();

            Vector2 closestPoint = (Vector2)thisAgent.transform.position + direction * ahit.distance;

            Vector2 targetPosition = ahit.point + ahit.normal * (avoidDistance + thisAgent.Radius);

            return seek.GetSteering(thisAgent, targetPosition);
        }
    }
}


/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.Utilities;

namespace JStuff.AI.Steering
{
    [CreateAssetMenu(menuName = "JStuff/AI/Steering/Obstacle Avoidance 2D")]
    public class ObstacleAvoidance2D : SteeringPositionSO
    {
        [Min(0)]
        public float avoidDistance = 0;
        [Min(0.01f)]
        public float weight = 1;
        [Min(0.01f)]
        public float lookahead = 0.5f;

        private Seek seek;

        private void OnEnable()
        {
            seek = CreateInstance<Seek>();
        }

        public override SteeringOutput GetSteering(ISteeringAgent thisAgent, Vector3 targetTransform)
        {
            Vector2 direction = thisAgent.Velocity.normalized;

            RaycastHit2D[] hits =
                Physics2D.CircleCastAll(thisAgent.transform.position, thisAgent.Radius + avoidDistance, direction, lookahead);

            if (hits.Length == 0)
                return new SteeringOutput();

            RaycastHit2D ahit = hits[0];
            bool hasHit = false;
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.transform == thisAgent.CollisionTransform)
                    continue;

                ahit = hit;

                hasHit = true;
            }

            if (!hasHit)
                return new SteeringOutput();

            Vector2 closestPoint = (Vector2)thisAgent.transform.position + direction * ahit.distance;

            Vector2 targetPosition = closestPoint + ahit.normal * (avoidDistance + thisAgent.Radius);

            return seek.GetSteering(thisAgent, targetPosition);
        }
    }
}
*/

/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.Utilities;

namespace JStuff.AI.Steering
{
    [CreateAssetMenu(menuName = "JStuff/AI/Steering/Obstacle Avoidance 2D")]
    public class ObstacleAvoidance2D : SteeringPositionSO
    {
        [Min(0)]
        public float avoidDistance = 0;
        [Min(0.01f)]
        public float weight = 1;
        [Min(0.01f)]
        public float lookahead = 0.5f;

        private Seek seek;

        private void OnEnable()
        {
            seek = CreateInstance<Seek>();
        }

        public override SteeringOutput GetSteering(ISteeringAgent thisAgent, Vector3 targetTransform)
        {
            Vector2 direction = thisAgent.Velocity.normalized;
            Vector2 ray = direction * lookahead;

            Collider2D[] hits =
                Physics2D.OverlapCircleAll((Vector2)thisAgent.transform.position + ray, thisAgent.Radius + avoidDistance);

            if (hits.Length == 0)
                return new SteeringOutput();

            Collider2D ahit = hits[0];
            bool hasHit = false;
            foreach (Collider2D hit in hits)
            {
                if (hit.transform == thisAgent.CollisionTransform)
                    continue;

                ahit = hit;

                hasHit = true;
            }

            if (!hasHit)
                return new SteeringOutput();

            ColliderDistance2D colliderDistance2D = ahit.

                ahit.Cast

            Vector2 closestPoint = ahit.ClosestPoint(thisAgent.transform.position);

            Vector2 targetPosition = closestPoint + ahit. * (avoidDistance + thisAgent.Radius);

            return seek.GetSteering(thisAgent, targetPosition);
        }
    }
}
*/

/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.Utilities;

namespace JStuff.AI.Steering
{
    [CreateAssetMenu(menuName ="JStuff/AI/Steering/Avoidance 2D")]
    public class Avoidance2D : SteeringPositionSO
    {
        [Min(0.1f)]
        public float avoidDistance = 1;

        public override SteeringOutput GetSteering(ISteeringAgent thisAgent, Vector3 targetTransform)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(thisAgent.transform.position, thisAgent.Radius + avoidDistance);

            Vector2 targetVelocity = Vector2.zero;
            Vector2 pos = thisAgent.transform.position;

            foreach (Collider2D hit in hits)
            {
                if (hit.transform.position == targetTransform || hit.transform == thisAgent.transform)
                    continue;

                Vector2 closestPoint = hit.ClosestPoint(pos);
                float distance = (pos - closestPoint).magnitude;

                targetVelocity += (pos - closestPoint).normalized * distance.Remap(thisAgent.Radius, avoidDistance, 1, 0).Clamp(0,1);
            }

            return new SteeringOutput(targetVelocity * thisAgent.MaxAcceleration);
        }
    }
}
*/