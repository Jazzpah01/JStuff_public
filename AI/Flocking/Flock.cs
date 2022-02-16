using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.AI.Flocking
{
    /// <summary>
    /// based on: https://www.youtube.com/watch?v=mjKINQigAE4
    /// </summary>
    public class Flock : MonoBehaviour
    {
        [SerializeField]
        private WeightedBehavior[] weightedBehaviors;

        [SerializeField]
        private List<Boid> boids;

        [SerializeField]
        private float maxSpeed;

        public List<Boid> Boids
        {
            get { return boids; }
        }

        void Awake()
        {
            // Initialize boids
            boids = new List<Boid>();
            List<Transform> boidTransforms = new List<Transform>();
            foreach (Boid b in this.gameObject.GetComponentsInChildren<Boid>())
            {
                boids.Add(b);
                boidTransforms.Add(b.transform);
            }

            // Initialize behaviors
            for (int i = 0; i < weightedBehaviors.Length; i++)
            {
                if (weightedBehaviors[i].FlockAsContext)
                {
                    weightedBehaviors[i].SetContext(boidTransforms);
                } else
                {
                    weightedBehaviors[i].SetContext(new List<Transform>());
                }
            }
        }

        void Update()
        {
            foreach(Boid boid in boids)
            {
                Vector2 v = Vector2.zero;
        
                foreach(WeightedBehavior b in weightedBehaviors)
                {
                    v += b.Behavior.VelocityChange(this, boid, b.Context) * b.Weight;
                }
        
                boid.velocity = v.normalized * maxSpeed;
            }
        
            foreach(Boid boid in boids)
            {
                boid.UpdatePosition();
            }
        }

        public void AddContext(int index, Transform t)
        {
            weightedBehaviors[index].AddContext(t);
        }

        public void AddContext(FlockingBehavior behavior, Transform t)
        {
            for (int i = 0; i < weightedBehaviors.Length; i++)
            {
                if (weightedBehaviors[i].Behavior == behavior)
                {
                    weightedBehaviors[i].AddContext(t);
                }
            }
        }

        public void RemoveContext(int index, Transform t)
        {
            weightedBehaviors[index].RemoveContext(t);
        }

        public void RemoveContext(FlockingBehavior behavior, Transform t)
        {
            for (int i = 0; i < weightedBehaviors.Length; i++)
            {
                if (weightedBehaviors[i].Behavior == behavior)
                {
                    weightedBehaviors[i].RemoveContext(t);
                }
            }
        }

        public void AddBoid(Boid b)
        {
            boids.Add(b);
            b.transform.parent = this.transform;
            for (int i = 0; i < weightedBehaviors.Length; i++)
            {
                if (weightedBehaviors[i].FlockAsContext)
                {
                    weightedBehaviors[i].AddContext(b.transform);
                }
            }
        }

        public void RemoveBoid(Boid b)
        {
            boids.Remove(b);
            for (int i = 0; i < weightedBehaviors.Length; i++)
            {
                if (weightedBehaviors[i].FlockAsContext)
                {
                    weightedBehaviors[i].RemoveContext(b.transform);
                }
            }
        }



        public (Boid, float) NearestBoid(Boid boid)
        {
            Boid retval = null;
            float distance = float.MaxValue;
            foreach (Boid b in boids)
            {
                Vector2 v = b.transform.position - boid.transform.position;
                if (v.magnitude < distance
                    && b != boid)
                {
                    retval = b;
                    distance = v.magnitude;
                }
            }
            return (retval, distance);
        }

        public List<Boid> BoidsInRadius(Vector2 point, float radius)
        {
            List<Boid> retval = new List<Boid>();
            foreach (Boid boid in boids)
            {
                if ((boid.transform.position - (Vector3)point).magnitude <= radius)
                    retval.Add(boid);
            }
            return retval;
        }
    }
}