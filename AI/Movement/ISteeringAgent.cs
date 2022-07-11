using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.Utilities;

namespace JStuff.AI.Steering
{
    public interface ISteeringAgent: IComponent, ICircleCollider
    {
        float MaxSpeed { get; }
        float MaxAcceleration { get; }
        float MaxRotation { get; }
        float Drag { get; }
        float SpeedThreshold { get; }
        Vector2 Velocity { get; }
        float Orientation { get; }
        UpAxis Up { get; }
        Transform CollisionTransform { get; }
    }
}