using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace JStuff.AI.Steering
{
    [CreateAssetMenu(menuName = "JStuff/AI/Steering/Seek")]
    public class Seek : SteeringPositionSO
    {
        public override SteeringOutput GetSteering(ISteeringAgent agent, Vector2 target)
        {
            Vector2 direction = target - agent.Position;

            Vector2 acceleration = direction.normalized * agent.MaxAcceleration;

            return new SteeringOutput(acceleration);
        }

        public static Seek Create()
        {
            return CreateInstance<Seek>();
        }
    }
}