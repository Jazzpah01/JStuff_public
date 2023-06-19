using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.AI.Steering
{
    [CreateAssetMenu(menuName ="JStuff/AI/Steering/Agent Avoidance")]
    public class AgentAvoidance : SteeringSO
    {
        public float avoidDistance;

        private Seek seek;

        private void OnEnable()
        {
            seek = CreateInstance<Seek>();
        }

        public override SteeringOutput GetSteering(ISteeringAgent agent)
        {
            Vector2 target = agent.Position;
            bool touching = false;
            float d = float.MaxValue;

            foreach (ISteeringAgent other in NavigationSystem.instance.GetAgents())
            {
                if (agent == other)
                    continue;

                float nd = Vector2.Distance(agent.Position, other.Position);

                if (nd < d)
                {
                    d = nd;
                }

                if (nd < avoidDistance + agent.Radius + other.Radius)
                {
                    target += agent.Position - other.Position;
                    touching = true;
                }
            }

            if (touching)
                return seek.GetSteering(agent, agent.Position + target) * Mathf.Clamp01(d / avoidDistance);

            return new SteeringOutput();
        }
    }
}