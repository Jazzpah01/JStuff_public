using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.AI.Steering
{
    public class CompositeSteering : ISteeringBehavior<bool>
    {
        Func<SteeringOutput> a;
        Func<SteeringOutput> b;

        public CompositeSteering(Func<SteeringOutput> a, Func<SteeringOutput> b)
        {
            this.a = a;
            this.b = b;
        }

        public SteeringOutput GetSteering(ISteeringAgent agent, bool target)
        {
            return a() + b();
        }
    }
}