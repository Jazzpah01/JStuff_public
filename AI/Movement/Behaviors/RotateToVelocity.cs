using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.VectorSwizzling;
using JStuff.Utilities;

namespace JStuff.AI.Steering
{
    [CreateAssetMenu(menuName ="JStuff/AI/Steering/Rotate To Velocity")]
    public class RotateToVelocity : SteeringSO
    {
        //https://stackoverflow.com/questions/1878907/how-can-i-find-the-difference-between-two-angles
        //https://gamedev.stackexchange.com/questions/4467/comparing-angles-and-working-out-the-difference
        public override SteeringOutput GetSteering(ISteeringAgent agent)
        {
            if (agent.Velocity == Vector2.zero)
            {
                return new SteeringOutput(Vector3.zero, 0);
            }

            float currentOrientation = 0;
            float targetOrientation = 0;

            currentOrientation = agent.Orientation;
            targetOrientation = agent.Velocity.Orientation();

            float delta = (targetOrientation - currentOrientation + 540) % 360 - 180;

            return new SteeringOutput(Vector3.zero, Mathf.Clamp(delta * agent.MaxRotation, -agent.MaxRotation, agent.MaxRotation));
        }
    }
}