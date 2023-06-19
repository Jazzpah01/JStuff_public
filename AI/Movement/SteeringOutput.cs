using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.AI.Steering
{
    public struct SteeringOutput
    {
        public Vector2 linear;
        public float angular;

        public SteeringOutput(Vector2 linear, float angular)
        {
            this.linear = linear;
            this.angular = angular;
        }
        public SteeringOutput(Vector2 linear)
        {
            this.linear = linear;
            this.angular = 0;
        }

        public SteeringOutput(float angular)
        {
            this.linear = Vector3.zero;
            this.angular = angular;
        }

        public void SetBounds(float acceleration, float rotation)
        {
            this.linear = linear.normalized * acceleration;
            this.angular = Mathf.Clamp(this.angular, -rotation, rotation);
        }

        public SteeringOutput GetBounded(float acceleration, float rotation)
        {
            return new SteeringOutput(linear.normalized * acceleration, Mathf.Clamp(this.angular, -rotation, rotation));
        }

        public override string ToString()
        {
            return $"SteeringOutput. Linear: {linear}. Angular: {angular}.";
        }

        public static SteeringOutput operator +(SteeringOutput a, SteeringOutput b) => new SteeringOutput(a.linear + b.linear, a.angular + b.angular);
        public static SteeringOutput operator -(SteeringOutput a, SteeringOutput b) => new SteeringOutput(a.linear - b.linear, a.angular - b.angular);
        public static SteeringOutput operator *(SteeringOutput a, float s) => new SteeringOutput(a.linear * s, a.angular * s);
        public static SteeringOutput operator /(SteeringOutput a, float s) => new SteeringOutput(a.linear / s, a.angular / s);
        public static bool operator ==(SteeringOutput a, SteeringOutput b) => (a.angular == b.angular && a.linear == b.linear);
        public static bool operator !=(SteeringOutput a, SteeringOutput b) => (a.angular != b.angular || a.linear != b.linear);
    }
}