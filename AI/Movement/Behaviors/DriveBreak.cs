using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.AI.Steering
{
    [CreateAssetMenu(menuName = "JStuff/AI/Steering/Drive/Break")]
    public class DriveBreak : SteeringPositionSO
    {
        public override SteeringOutput GetSteering(ISteeringAgent agent, Vector2 target)
        {
            Vector2 targetVelocity = Vector2.zero;

            Vector2 acceleration = (targetVelocity - agent.Velocity).normalized * agent.MaxAcceleration;

            if (acceleration.magnitude > agent.MaxAcceleration)
            {
                acceleration = acceleration.normalized * agent.MaxAcceleration;
            }

            return new SteeringOutput(acceleration);
        }
    }
}