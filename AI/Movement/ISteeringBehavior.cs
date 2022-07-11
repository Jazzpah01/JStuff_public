using JStuff.AI.Steering;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISteeringBehavior<T>
{
    SteeringOutput GetSteering(ISteeringAgent agent, T target);
}

public interface ISteeringBehavior
{
    SteeringOutput GetSteering(ISteeringAgent agent);
}