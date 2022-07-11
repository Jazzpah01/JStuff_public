using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.AI.Steering
{
    [CreateAssetMenu(menuName = "JStuff/AI/Steering/Flee")]
    public class Flee : SteeringPositionSO
    {
        public override SteeringOutput GetSteering(ISteeringAgent agent, Vector2 target)
        {
            if (target == null)
                return new SteeringOutput();

            Vector2 direction = agent.Position - target;

            Vector2 targetVelocity = direction.normalized * agent.MaxSpeed;

            Vector2 acceleration = (targetVelocity - agent.Velocity).normalized * agent.MaxAcceleration;

            return new SteeringOutput(acceleration);
        }
    }
}