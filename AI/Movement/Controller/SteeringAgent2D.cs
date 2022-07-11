using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.VectorSwizzling;
using System;

namespace JStuff.AI.Steering
{
    public class SteeringAgent2D : MonoBehaviour, ISteeringAgent
    {
        [Header("Attributes")]
        public float speed = 10;
        public float maxAcceleration = 50;
        public float maxNormalRotation = 1;
        public float radius = 1;
        public float drag = 0.1f;
        public float speedThreshold = 0;
        public float defaultBehaviorWeight = 1;

        public UpAxis upAxis = UpAxis.Z;

        public SteeringSO defaultBehavior;

        [Header("References")]
        public Collider2D collider;

        [Header("Runtime")]
        public float orientation;
        public Vector3 velocity = Vector3.zero;
        public Func<SteeringOutput> steeringTask;

        public Vector2 Position => transform.position.xy();
        public float Radius => radius;
        public Vector2 MinPosition => transform.position.xy() - new Vector2(Radius, Radius);
        public Vector2 MaxPosition => transform.position.xy() + new Vector2(Radius, Radius);
        public float MaxSpeed => speed;
        public float MaxAcceleration => maxAcceleration;
        public float MaxRotation => maxNormalRotation * 360;
        public float Drag => drag;
        public float SpeedThreshold => speedThreshold;
        public Vector2 Velocity => velocity;
        public float Orientation => orientation;
        public UpAxis Up => upAxis;

        public Transform CollisionTransform => collider.transform;

        private void Awake()
        {

        }

        private void Update()
        {
            SteeringOutput change = new SteeringOutput();

            // Get weighted steering
            if (defaultBehavior != null)
            {
                change = defaultBehavior.GetSteering(this);//.GetBounded(MaxAcceleration, MaxRotation);
                change *= defaultBehaviorWeight;
            }

            if (steeringTask != null)
            {
                change += steeringTask();//.GetBounded(MaxAcceleration, MaxRotation);
            }

            Debug.Log("Result: " + change);

            change.SetBounds(MaxAcceleration, MaxRotation);

            velocity += change.linear * Time.deltaTime;
            orientation +=  change.angular * Time.deltaTime;
            float currentSpeed = velocity.magnitude;

            if (currentSpeed > speed)
            {
                velocity = velocity.normalized * speed;
            }

            if (orientation > 360) orientation -= 360;
            if (orientation < 0) orientation += 360;

            if (change.linear == Vector3.zero)
            {
                velocity -= velocity * drag;
            }

            if (currentSpeed > speedThreshold)
                transform.position = transform.position + velocity * Time.deltaTime;

            transform.rotation = Quaternion.Euler(0, 0, orientation);
        }

        public void SetTask<T>(ISteeringBehavior<T> behavior, T target)
        {
            this.steeringTask = delegate { return behavior.GetSteering(this, target); };
        }

        public void SetTask(Func<SteeringOutput> steeringTask)
        {
            this.steeringTask = steeringTask;
        }

        public void SetTask()
        {
            this.steeringTask = null;
        }
    }
}