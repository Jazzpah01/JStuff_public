using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.TwoD.Platformer
{
    public class SimpleController : MonoBehaviour
    {
        [SerializeField]
        private float speed;

        void Update()
        {
            Vector2 pos = this.transform.position;
            if (Input.GetKey(KeyCode.DownArrow))
                pos.y -= speed * Time.deltaTime;
            if (Input.GetKey(KeyCode.UpArrow))
                pos.y += speed * Time.deltaTime;
            if (Input.GetKey(KeyCode.RightArrow))
                pos.x += speed * Time.deltaTime;
            if (Input.GetKey(KeyCode.LeftArrow))
                pos.x -= speed * Time.deltaTime;
            this.transform.position = pos;
        }
    }
}