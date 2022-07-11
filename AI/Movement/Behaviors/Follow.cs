using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.AI.Steering
{
    public class Follow : SteeringPositionSO
    {
        float distance;

        public override SteeringOutput GetSteering(ISteeringAgent thisAgent, Vector2 target)
        {
            throw new System.Exception();
        }
    }
}