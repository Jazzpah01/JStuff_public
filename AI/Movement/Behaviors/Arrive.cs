using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.VectorSwizzling;

namespace JStuff.AI.Steering
{
    [CreateAssetMenu(menuName = "JStuff/AI/Steering/Arrive")]
    public class Arrive : SteeringPositionSO
    {
        public float stopRadius;
        public float slowRadius;

        public override SteeringOutput GetSteering(ISteeringAgent agent, Vector2 target)
        {
            if (target == null)
                return new SteeringOutput();

            Vector2 position = agent.Position;

            SteeringOutput retval = new SteeringOutput();
            Vector2 direction = target - position;
            float distance = direction.magnitude;
            float acceleration = agent.MaxAcceleration;
            float targetSpeed = 0;
            float maxSpeed = agent.MaxSpeed;

            if (distance > slowRadius)
            {
                // Outside slow, so accelerate
                targetSpeed = maxSpeed;
            }
            else if (distance > stopRadius)
            {
                // Inside slow
                targetSpeed = maxSpeed * distance / slowRadius;
            }
            else
            {
                // Stop
                return new SteeringOutput();
            }

            Vector2 targetVelocity = direction.normalized * targetSpeed;

            retval.linear = (targetVelocity - agent.Velocity).normalized * acceleration;

            if (retval.linear.magnitude > acceleration)
            {
                retval.linear = retval.linear.normalized * acceleration;
            }

            retval.angular = 0;
            return retval;
        }
    }
}