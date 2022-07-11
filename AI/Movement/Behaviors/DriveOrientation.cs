using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.Utilities;
using JStuff.VectorSwizzling;

namespace JStuff.AI.Steering
{
    [CreateAssetMenu(menuName ="JStuff/AI/Steering/Drive/Orientation")]
    public class DriveOrientation : SteeringPositionSO
    {
        public float maxSpeedRotationFactor = 0.1f;
        public float minSpeedRotationFactor = 1f;

        public float minSpeed = 0.1f;

        public override SteeringOutput GetSteering(ISteeringAgent agent, Vector2 target)
        {
            if (target == null || agent.Velocity.magnitude < minSpeed)
                return new SteeringOutput();

            Vector2 targetDirection = target - agent.Position;

            float targetOrientation = targetDirection.normalized.xy().Orientation();

            float t = agent.Velocity.magnitude.Remap(0, agent.MaxSpeed, 0, 1).Clamp(0,1);

            Vector2 targetOrientationVector = targetOrientation.GetOrientationVector(agent.Up);

            float deltaOrientation = (targetOrientation - agent.Orientation + 540) % 360 - 180;

            float dot = Mathf.Abs(Vector3.Dot(targetOrientation.GetOrientationVector(agent.Up), agent.Velocity.normalized));

            float maxRotation = Mathf.Lerp(minSpeedRotationFactor, maxSpeedRotationFactor, t) * agent.MaxRotation;
            Debug.Log(maxRotation);

            if (dot > 0.99f)
                return new SteeringOutput(deltaOrientation * maxRotation);

            float maxSpeed = agent.MaxSpeed;// * Mathf.SmoothStep(0, 1, dot);
            Vector2 targetVelocity = (targetOrientationVector).normalized * Mathf.SmoothStep(0, 1, dot) * (agent.Velocity.magnitude - agent.Velocity.magnitude * agent.Drag);

            Vector2 acceleration = (targetVelocity - agent.Velocity).normalized * agent.MaxAcceleration;

            
            return new SteeringOutput(acceleration, deltaOrientation * maxRotation);
        }
    }
}


//public override SteeringOutput GetSteering(ISteeringAgent agent, Transform target)
//{
//    if (target == null || agent.Velocity.magnitude < minSpeed)
//        return new SteeringOutput();

//    Vector3 targetDirection = target.position - agent.transform.position;

//    float targetOrientation = targetDirection.normalized.xy().Orientation();

//    float t = agent.Velocity.magnitude.Remap(0, agent.MaxSpeed, 0, 1).Clamp(0, 1);

//    float maxRotation = Mathf.Lerp(minSpeedRotationFactor, maxSpeedRotationFactor, t) * agent.MaxRotation;

//    float deltaOrientation = (targetOrientation - agent.Orientation + 540) % 360 - 180;

//    return new SteeringOutput(deltaOrientation * maxRotation);
//}