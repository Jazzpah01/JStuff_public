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

        GameObject visual;

        private void OnEnable()
        {
            seek = CreateInstance<Seek>();
        }

        public override SteeringOutput GetSteering(ISteeringAgent agent, IList<Vector2> path)
        {
            if ((path[path.Count - 1] - agent.Position).sqrMagnitude < stopDistance * stopDistance)
            {
                return new SteeringOutput();
            }

            Vector2 point = path[0];
            float dist = float.MaxValue;
            Vector2 dir = path[1] - path[0];

            for (int i = 1; i < path.Count; i++)
            {
                Vector2 npoint = NearestPointOnLine(path[i - 1], path[i], agent.Position);
                float ndist = (npoint - agent.Position).magnitude;
                if (ndist < dist)
                {
                    dist = ndist;
                    point = npoint;
                    dir = path[i] - path[i - 1];
                }
            }

            Vector2 target = point + dir.normalized * offset;

            //if (visual == null)
            //{
            //    visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //    MonoBehaviour.Destroy(visual.GetComponent<Collider>());
            //}
            //visual.transform.position = new Vector3(target.x, 0, target.y);

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
            float d = Vector2.Dot(v, lineDir);
            Vector2 candidate = linePnt0 + lineDir * d;

            float canDist = (candidate - linePnt0).magnitude;

            if (d < 0)
            {
                return linePnt0;
            }
            else if (d < lineLength)
            {
                return candidate;
            } else {
                return linePnt1;
            }
        }
    }
}