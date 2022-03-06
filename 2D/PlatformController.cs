using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff;

namespace JStuff.TwoD.Platformer
{
    public class PlatformController : MonoBehaviour
    {
        public Body2d body;
        public BaseModifier running;
        public BaseModifier crouch;
        public BaseModifier bouncy;

        private bool bouncing = false;

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (Input.GetKey(KeyCode.Space) && !bouncing)
                {
                    body.ApplyFilter(bouncy);
                    bouncing = true;
                }
                body.JumpInput(1);
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                body.HorizontalInput(-1);
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                body.HorizontalInput(1);
            }
            if (Input.GetKeyUp(KeyCode.UpArrow))
            {
                body.FallInput();
            }

            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                body.ApplyFilter(running);
            }
            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                body.RemoveFilter(running);
            }


            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                body.ApplyFilter(crouch);
            }
            if (Input.GetKeyUp(KeyCode.LeftControl))
            {
                body.RemoveFilter(crouch);
            }

            if (Input.GetKeyUp(KeyCode.Space) && bouncing)
            {
                body.RemoveFilter(bouncy);
                bouncing = false;
            }
        }
    }
}