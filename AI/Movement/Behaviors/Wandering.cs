using JStuff.AI.Steering;
using JStuff.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.Utilities;

[CreateAssetMenu(menuName = "JStuff/AI/Steering/Wandering")]
public class Wandering : SteeringPositionSO
{
    public float max_delta_dregees = 10;

    public override SteeringOutput GetSteering(ISteeringAgent agent, Vector2 nan)
    {
        float r = Random.value.Remap(0f, 1f, -1f, 1f);

        float currentOrientation = agent.Orientation + max_delta_dregees * r;
        Vector2 target = Utilities.GetDirection(currentOrientation) + agent.Position;

        return Seek.Create().GetSteering(agent, target);
    }
}