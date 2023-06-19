using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.Utilities;

namespace JStuff.AI.Steering
{
    public class DriveSteering : MonoBehaviour, ISteeringBehavior<Transform>
    {
        private SteeringAgent2D agent;

        public DriveAccelerate driveAcceleration;
        public DriveBreak driveBreak;
        public DriveOrientation driveOrientation;

        public Transform target;

        public float steerFactor = 1f;
        public float accFactor = 0.5f;

        [Header("Runtime")]
        public float accelerate = 0;
        public float steer = 0;
        public float breakk = 0;

        public SteeringOutput GetSteering(ISteeringAgent agent, Transform target)
        {
            SteeringOutput retval = new SteeringOutput();

            retval += driveBreak.GetSteering(agent, this.target.position) * breakk;

            if (breakk == 0)
            {
                retval += driveAcceleration.GetSteering(agent, this.target.position) * accelerate;
            }
            
            retval += driveOrientation.GetSteering(agent, this.target.position);

            return retval;
        }

        private void Start()
        {
            agent = GetComponent<SteeringAgent2D>();
            agent.SetBehavior<Transform>(this, target);
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                steer += steerFactor * Time.deltaTime;
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                steer -= steerFactor * Time.deltaTime;
            }
            else
            {
                steer = steer.MoveTo(0, steerFactor * Time.deltaTime);
            }

            if (Input.GetKey(KeyCode.UpArrow))
            {
                accelerate += accFactor * Time.deltaTime;
            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                accelerate -= accFactor * Time.deltaTime;
            }
            else
            {
                accelerate = accelerate.MoveTo(0, accFactor * Time.deltaTime);
            }

            breakk = 0;
            if (Input.GetKey(KeyCode.Space))
            {
                breakk = 1;
            }

            accelerate = Mathf.Clamp01(accelerate);
            steer = Mathf.Clamp(steer, -1, 1);

            target.localPosition = new Vector3(target.localPosition.x, steer, target.localPosition.z);
        }
    }
}