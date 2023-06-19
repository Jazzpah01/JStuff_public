using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.VectorSwizzling;
using System;

namespace JStuff.AI.Steering
{
    public class SteeringAgent2D : SteeringAgent
    {
        public override Vector2 MinPosition => transform.position.xy() - new Vector2(Radius, Radius);
        public override Vector2 MaxPosition => transform.position.xy() + new Vector2(Radius, Radius);

        public override UpAxis Up => UpAxis.Z;

        public override Vector2 Position => transform.position.xy();

        private void Update()
        {
            // Check if target is within reach
            if (steeringTask != null && !steeringTask.ShouldFollow())
            {
                if (callback != null && !hasReached)
                {
                    callback();
                    hasReached = true;
                }
                else
                {
                    hasReached = false;
                }
                if (!hug)
                    SetTarget();
            }

            SteeringOutput change = GetSteering();

            UpdateSteering(change);

            float currentSpeed = velocity.magnitude;

            if (currentSpeed > speedThreshold)
                transform.position = transform.position.xy() + velocity * Time.deltaTime;

            transform.rotation = Quaternion.Euler(0, 0, orientation);
        }
    }
}