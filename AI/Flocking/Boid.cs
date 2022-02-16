using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.AI.Flocking
{
    public class Boid : MonoBehaviour
    {
        public Vector3 velocity = new Vector3();
        public virtual void UpdatePosition()
        {
            this.transform.position += velocity * Time.deltaTime;
        }

        public virtual void InitializeBoid()
        {
            throw new System.Exception("Method not implemented.");
        }

        public virtual void RemoveBoid()
        {
            throw new System.Exception("Method not implemented.");
        }
    }
}