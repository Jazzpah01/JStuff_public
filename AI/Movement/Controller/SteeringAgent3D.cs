using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.VectorSwizzling;
using System;

namespace JStuff.AI.Steering
{
    public class SteeringAgent3D : SteeringAgent
    {
        public override Vector2 MinPosition => transform.position.xz() - new Vector2(Radius, Radius);
        public override Vector2 MaxPosition => transform.position.xz() + new Vector2(Radius, Radius);

        public override UpAxis Up => UpAxis.Y;

        public override Vector2 Position => transform.position.xz();

        private Vector2 lasPosition;

        protected virtual void FixedUpdate()
        {
            // Check if target is within reach
            if (steeringTask != null && !steeringTask.ShouldFollow())
            {
                if (callback != null && !hasReached)
                {
                    callback();
                    hasReached = true;
                } else
                {
                    hasReached = false;
                }
                if (!hug)
                    SetTarget();
            }

            SteeringOutput change = GetSteering();

            UpdateSteering(change);

            //float currentSpeed = velocity.magnitude;



            //if (currentSpeed > speedThreshold)
            //{
            //    if (currentSpeed * Time.fixedDeltaTime > radius)
            //    {
            //        rigidbody.position = rigidbody.position + new Vector3(velocity.x, 0, velocity.y) / currentSpeed * radius;
            //        //transform.position = transform.position + new Vector3(velocity.x, 0, velocity.y) / currentSpeed * radius;
            //    }
            //    else
            //    {
            //        rigidbody.position = rigidbody.position + new Vector3(velocity.x, 0, velocity.y) * Time.fixedDeltaTime;
            //        //transform.position = transform.position + new Vector3(velocity.x, 0, velocity.y) * Time.fixedDeltaTime;
            //    }
            //}

            ////rigidbody.AddForce(change.linear);

            //rigidbody.rotation = Quaternion.Euler(0, -(orientation - 90), 0);
            ////transform.rotation = Quaternion.Euler(0, orientation, 0);

            //if (rigidbody == null)
            //{
            //    rigidbody = GetComponent<Rigidbody>();
            //} else
            //{
            //    rigidbody.velocity = Vector3.zero;
            //}

            ////if (lasPosition != Vector2.zero)
            ////    velocity = Position - lasPosition;
            ////lasPosition = Position;
        }
    }
}