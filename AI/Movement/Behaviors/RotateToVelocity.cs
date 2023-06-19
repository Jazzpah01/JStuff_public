using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.VectorSwizzling;
using JStuff.Utilities;

namespace JStuff.AI.Steering
{
    [CreateAssetMenu(menuName = "JStuff/AI/Steering/Rotate To Velocity")]
    public class RotateToVelocity : SteeringSO
    {
        public float factor = 1f;

        //https://stackoverflow.com/questions/1878907/how-can-i-find-the-difference-between-two-angles
        //https://gamedev.stackexchange.com/questions/4467/comparing-angles-and-working-out-the-difference
        public override SteeringOutput GetSteering(ISteeringAgent agent)
        {
            if (agent.Velocity == Vector2.zero)
            {
                return new SteeringOutput(Vector2.zero, 0);
            }

            float currentOrientation = agent.Orientation;
            float targetOrientation = agent.Velocity.GetOrientation();

            float delta = Utilities.Utilities.OrientationDifference(currentOrientation, targetOrientation);

            return new SteeringOutput(Vector2.zero, delta * factor);
        }
    }
}