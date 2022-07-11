using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.VectorSwizzling;

namespace JStuff.AI.Steering
{
    public abstract class SteeringPositionSO : ScriptableObject, ISteeringBehavior<Vector2>, ISteeringBehavior<Transform>
    {
        public abstract SteeringOutput GetSteering(ISteeringAgent agent, Vector2 target);
        public SteeringOutput GetSteering(ISteeringAgent agent, Transform target) =>
            agent.Up == UpAxis.Y ? GetSteering(agent, target.position.xz()) : GetSteering(agent, target.position.xy());
    }
}