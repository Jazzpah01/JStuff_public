using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.AI.Steering
{
    [CreateAssetMenu(menuName ="JStuff/AI/Steering/Follow Path")]
    public class FollowPath : SteeringPathSO
    {
        public float offset;
        public float stopDistance;

        private Seek seek;

        private void OnEnable()
        {
            seek = CreateInstance<Seek>();
        }

        public override SteeringOutput GetSteering(ISteeringAgent agent, IList<Vector2> path)
        {
            if ((path[path.Count-1] - agent.Position).sqrMagnitude < stopDistance * stopDistance)
            {
                return new SteeringOutput();
            }

            Vector2 point = Vector2.zero;
            float dist = float.MaxValue;
            Vector2 dir = Vector2.zero;

            for (int i = 1; i < path.Count; i++)
            {
                Vector2 npoint = NearestPointOnLine(path[i - 1], path[i], agent.transform.position);
                float ndist = (point - agent.Position).sqrMagnitude;
                if (ndist < dist)
                {
                    dist = ndist;
                    point = npoint;
                    dir = path[i] - path[i - 1];
                }
            }

            return seek.GetSteering(agent, point + dir.normalized * offset);
        }


        //linePnt - point the line passes through
        //lineDir - unit vector in direction of line, either direction works
        //pnt - the point to find nearest on line SEGMENT for
        //https://forum.unity.com/threads/how-do-i-find-the-closest-point-on-a-line.340058/
        public static Vector2 NearestPointOnLine(Vector2 linePnt0, Vector2 linePnt1, Vector2 pnt)
        {
            Vector2 lineDir = linePnt1 - linePnt0;
            float lineLength = lineDir.magnitude;
            lineDir /= lineLength;

            Vector2 v = pnt - linePnt0;
            float d = Vector3.Dot(v, lineDir);
            Vector2 candidate = linePnt0 + lineDir * d;

            float canDist = (candidate - linePnt0).sqrMagnitude;

            if (d < 0)
            {
                return linePnt0;
            }
            else if (canDist <= lineLength * lineLength)
            {
                return candidate;
            } else {
                return linePnt1;
            }
        }
    }
}