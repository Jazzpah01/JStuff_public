using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.AI.Steering
{
    [CreateAssetMenu(menuName = "JStuff/AI/Steering/Drive/Accelerate")]
    public class DriveAccelerate : SteeringPositionSO
    {
        public override SteeringOutput GetSteering(ISteeringAgent agent, Vector2 target)
        {
            Vector2 direction = Utilities.Utilities.GetOrientationVector(agent.Orientation, agent.Up);

            Vector2 targetVelocity = direction.normalized * agent.MaxSpeed;

            Vector2 acceleration = (targetVelocity - agent.Velocity).normalized * agent.MaxAcceleration;

            if (acceleration.magnitude > agent.MaxAcceleration)
            {
                acceleration = acceleration.normalized * agent.MaxAcceleration;
            }

            return new SteeringOutput(acceleration);
        }
    }
}