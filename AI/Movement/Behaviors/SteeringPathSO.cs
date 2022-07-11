using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.AI.Steering
{
    public abstract class SteeringPathSO : ScriptableObject, ISteeringBehavior<IList<Vector2>>
    {
        public abstract SteeringOutput GetSteering(ISteeringAgent agent, IList<Vector2> Path);
    }
}