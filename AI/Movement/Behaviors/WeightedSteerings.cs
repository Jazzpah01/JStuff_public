using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.AI.Steering
{
    [CreateAssetMenu(menuName ="JStuff/AI/Steering/Composite Steering")]
    public class WeightedSteerings : SteeringSO
    {
        public List<WeightedSteering> steeringBehaviors;

        public override SteeringOutput GetSteering(ISteeringAgent thisAgent)
        {
            SteeringOutput retval = new SteeringOutput();

            foreach(WeightedSteering s in steeringBehaviors)
            {
                retval += s.behavior.GetSteering(thisAgent) * s.weight;
            }

            return retval;
        }
    }
}