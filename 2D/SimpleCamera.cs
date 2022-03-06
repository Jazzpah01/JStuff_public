using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.TwoD.Platformer
{
    public class SimpleCamera : MonoBehaviour
    {
        private void Update()
        {
            Vector3 camerapos = Camera.main.transform.position;
            camerapos.x = this.transform.position.x;
            Camera.main.transform.position = camerapos;
        }
    }
}