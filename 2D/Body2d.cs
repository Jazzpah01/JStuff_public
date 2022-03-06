using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.Utilities;
using JStuff.Collection;

namespace JStuff.TwoD.Platformer
{
    /// <summary>
    /// Based on: https://www.roystan.net/articles/character-controller-2d.html
    /// and: https://www.youtube.com/watch?v=7KiK0Aqtmzc
    /// and: https://www.caseportman.com/single-post/2018/08/16/input-buffering-2d-platforming
    /// </summary>
    public class Body2d : MonoBehaviour
    {
        [SerializeField]
        public Body2dData data;
        private Body2dData originalData;
        [SerializeField]
        private BoxCollider2D boxCollider;
        [SerializeField]
        private bool collideWithPlatform = false;

        public float initialGravityAcceleration;

        private Vector2 velocity;

        public bool grounded;
        private bool ceiled;

        private float horizontalInput;
        private float verticalInput;
        private bool jumpInput;

        private bool ascending = false;
        private bool descending = false;

        private bool oldGrounded = false;

        private Vector2 oldPosition;

        private int activeJackalFrames = 0;

        private int jumpInputBuffer = 0;
        private int fallInputBuffer = 0;
        private float jumpFactor = 0;

        private bool fall = false;

        private List<IBodyModifier> filters = new List<IBodyModifier>();

        public delegate void StateObserved(Body2d body);

        public bool GroundOnSpawn
        {
            set { data.groundOnSpawn = value; }
        }

        public bool CollideWithPlatform
        {
            get { return collideWithPlatform; }
            set { collideWithPlatform = value; }
        }

        private MultiPopper<string, Vector2> velocityStack =
            new MultiPopper<string, Vector2>((x, y) => x == y, (x, y) => x + y, "");

        public bool Ascending
        {
            get { return ascending; }
        }

        public bool Descending
        {
            get { return descending; }
        }

        public float GravetyAcceleration
        {
            set { data.gravityAcceleration = value; }
            get { return data.gravityAcceleration; }
        }

        public bool Grounded
        {
            get { return grounded; }
            private set
            {
                grounded = value;
                if (value)
                {
                    fall = (data.gravityAcceleration < 0) ? false : true;
                    velocity.y = data.verticalBounce * -velocity.y;
                    if (data.gravityAcceleration < 0)
                        activeJackalFrames = data.jackalFrames;
                    Debug.Log("BOUNCE");
                }
            }
        }

        public bool Ceiled
        {
            get { return ceiled; }
            set
            {
                ceiled = value;
                if (ceiled)
                {
                    fall = (data.gravityAcceleration > 0) ? false : true;
                    velocity.y = data.verticalBounce * -velocity.y;
                    if (data.gravityAcceleration > 0)
                        activeJackalFrames = data.jackalFrames;
                }
            }
        }

        public float JumpHeight
        {
            get { return data.jumpHeight; }
        }

        public void ApplyFilter(IBodyModifier filter)
        {
            for (int i = filters.Count - 1; i >= 0; i--)
            {
                filters[i].ApplyIvertedFilter(this);
            }

            data = Instantiate(originalData);

            filters.Add(filter);

            filters.Sort();

            for (int i = 0; i < filters.Count; i++)
            {
                filters[i].ApplyFilter(this);
            }
        }

        public void RemoveFilter(IBodyModifier filter)
        {
            for (int i = filters.Count - 1; i >= 0; i--)
            {
                filters[i].ApplyIvertedFilter(this);
            }

            data = Instantiate(originalData);

            filters.Remove(filter);

            filters.Sort();

            for (int i = 0; i < filters.Count; i++)
            {
                filters[i].ApplyFilter(this);
            }
        }

        public void AddVelocity(string key, Vector2 velocity)
        {
            velocityStack.Add(key, velocity);
        }
        public void AddVelocity(Vector2 velocity)
        {
            velocityStack.Add("input", velocity);
        }

        public void VerticalInput(float factor)
        {
            verticalInput = factor;
        }

        public void HorizontalInput(float factor)
        {
            horizontalInput = factor;
        }

        public void JumpInput(float factor)
        {
            jumpInputBuffer = data.inputBufferJump;
            jumpFactor = factor;
            fallInputBuffer = 0;
        }

        public void FallInput(bool useBuffer = true)
        {
            fall = true;
            if (useBuffer)
                fallInputBuffer = data.inputBufferJump;
            if (data.dropOnFall && ascending)
                velocity.y = 0;
        }

        public Vector2 Velocity
        {
            get { return new Vector2(velocity.x, velocity.y); }
        }

        void Start()
        {
            Cleanup();

            originalData = data;
            data = Instantiate(originalData);

            data.initialGravityAcceleration = GravetyAcceleration;

            if (boxCollider == null)
                boxCollider = gameObject.GetComponent<BoxCollider2D>();
            if (data.groundOnSpawn && boxCollider == null)
                throw new System.Exception("Cannot ground a body that has no box collider!");

            // Ground Body!
            if (!data.groundOnSpawn)
                return;
            grounded = false;
            oldGrounded = true;
            for (int i = 0; (i < 10 && !grounded && data.groundOnSpawn); i++)
            {
                // Materialize velocity
                transform.Translate(Vector2.down);

                // Detect collision
                Collider2D[] hits = ColliderController.OverlapBoxAllWithFlags(transform.position, boxCollider.size, data.ColliderFlags);
                Collider2D[] hitsPlatform = ColliderController.OverlapBoxAllWithFlags(transform.position, boxCollider.size, data.PlatformFlags);
                grounded = false;
                ceiled = false;

                // Handle collision
                foreach (Collider2D hit in hits)
                {
                    if (hit == boxCollider)
                        continue;

                    ColliderDistance2D colliderDistance = hit.Distance(boxCollider);

                    if (colliderDistance.isOverlapped)
                    {
                        transform.Translate(colliderDistance.pointA - colliderDistance.pointB);
                    }

                    // Is grounded
                    if (Vector2.Angle(colliderDistance.normal, Vector2.up) < 90 && velocity.y < 0 &&
                        Mathf.Abs(oldPosition.y - boxCollider.transform.position.y) < 0.0001f)
                    {
                        Grounded = true;
                    }

                    // Is ceiled
                    if (Vector2.Angle(colliderDistance.normal, Vector2.down) < 90 && velocity.y > 0 &&
                        Mathf.Abs(oldPosition.y - boxCollider.transform.position.y) < 0.0001f)
                    {
                        ceiled = true;
                    }
                }

                // Collision hit for platform
                // TODO: Make this work!!!
                foreach (Collider2D hit in hitsPlatform)
                {
                    if (hit == boxCollider || collideWithPlatform)
                        continue;

                    BoxCollider2D h = hit as BoxCollider2D;
                    if ((boxCollider.transform.position.x + boxCollider.size.x / 2 >
                        hit.transform.position.x - h.size.x / 2 ||
                        boxCollider.transform.position.x - boxCollider.size.x / 2 <
                        hit.transform.position.x + h.size.x / 2) &&
                        (boxCollider.transform.position.y - boxCollider.size.y / 2 >
                        hit.transform.position.y + h.size.y / 2) && descending)
                    {
                        ColliderDistance2D colliderDistance = hit.Distance(boxCollider);

                        if (colliderDistance.isOverlapped)
                        {
                            transform.Translate(colliderDistance.pointA - colliderDistance.pointB);
                            Grounded = true;
                        }
                    }
                }
            }
        }

        void FixedUpdate()
        {
            // Jump
            if (((Grounded && data.gravityAcceleration < 0) || activeJackalFrames > 0) &&
                jumpInputBuffer > 0)
            {
                velocity.y = 0;
                AddVelocity("jump", new Vector2(0, Mathf.Sqrt(2 * Mathf.Abs(JumpHeight) * jumpFactor * Mathf.Abs(GravetyAcceleration))));
                ascending = true;
                descending = false;
                fall = false;
                activeJackalFrames = 0;
                jumpInputBuffer = 0;
                Grounded = false;
            }
            else if (((Ceiled && data.gravityAcceleration > 0) || activeJackalFrames > 0) &&
                jumpInputBuffer > 0)
            {
                velocity.y = 0;
                AddVelocity("jump", -1 * new Vector2(0, Mathf.Sqrt(2 * Mathf.Abs(JumpHeight) * jumpFactor * Mathf.Abs(GravetyAcceleration))));
                descending = true;
                ascending = false;
                fall = false;
                activeJackalFrames = 0;
                jumpInputBuffer = 0;
                Ceiled = false;
            }

            if (fallInputBuffer > 0 && ((ascending && data.gravityAcceleration < 0) || (descending && data.gravityAcceleration > 0)))
            {
                fall = true;
                fallInputBuffer = 0;
            }

            if (activeJackalFrames > 0)
                activeJackalFrames--;

            if (jumpInputBuffer > 0)
                jumpInputBuffer--;

            if (fallInputBuffer > 0)
                fallInputBuffer--;

            velocity += velocityStack.MultiPop(new Vector2());

            // Applying gravety acceleration
            if (data.gravityAcceleration != 0)
            {
                velocity.y += data.gravityAcceleration * Time.fixedDeltaTime;
                if ((descending && data.gravityAcceleration < 0) || (ascending && data.gravityAcceleration > 0))
                {
                    if (data.fallOnDescend)
                        FallInput(false);
                    velocity.y += data.gravityAcceleration * Time.fixedDeltaTime * (data.GMDescend - 1);
                }
                if (fall)
                    velocity.y += data.gravityAcceleration * Time.fixedDeltaTime * (data.GMFall - 1);
            }

            // Apply movement
            if (horizontalInput != 0 &&
                        !(data.speed * horizontalInput < velocity.x && horizontalInput > 0) &&
                        !(data.speed * horizontalInput > velocity.x && horizontalInput < 0)) // BUG: should be able to airaccelerate
            {
                if ((Grounded && data.gravityAcceleration < 0) || (Ceiled && data.gravityAcceleration > 0))
                {
                    if (data.walkAcceleration != 0)
                        velocity.x = Mathf.MoveTowards(velocity.x, data.speed * horizontalInput, data.walkAcceleration * Time.fixedDeltaTime);
                }
                else
                {
                    if (data.airAcceleration != 0)
                        velocity.x = Mathf.MoveTowards(velocity.x, data.speed * horizontalInput, data.airAcceleration * Time.fixedDeltaTime);
                }
            }
            else // BUG
            {
                if ((Grounded && data.gravityAcceleration < 0) || (Ceiled && data.gravityAcceleration > 0))
                {
                    if (data.groundDeceleration != 0)
                        velocity.x = Mathf.MoveTowards(velocity.x, 0, data.groundDeceleration * Time.fixedDeltaTime);
                }
                else
                {
                    if (data.airAcceleration != 0)
                        velocity.x = Mathf.MoveTowards(velocity.x, 0, data.airAcceleration * Time.fixedDeltaTime);
                }
            }

            // Materialize velocity
            transform.Translate(velocity * Time.fixedDeltaTime);

            if (boxCollider == null)
                return;

            // Detect collision
            Vector3 boxOffset = new Vector2(boxCollider.offset.x, boxCollider.offset.y);
            Collider2D[] hits = ColliderController.OverlapBoxAllWithFlags(boxCollider.transform.position, boxCollider.size, data.ColliderFlags);
            Collider2D[] hitsPlatform = ColliderController.OverlapBoxAllWithFlags(boxCollider.transform.position, boxCollider.size, data.PlatformFlags);
            grounded = false;
            ceiled = false;

            Vector3 posBeforeCollision = boxCollider.transform.position;
            bool walled = false;

            // Handle collision
            foreach (Collider2D hit in hits)
            {
                if (hit == boxCollider)
                    continue;

                //Debug.Log("collission");

                ColliderDistance2D colliderDistance = hit.Distance(boxCollider);

                if (colliderDistance.isOverlapped)
                {
                    transform.Translate(colliderDistance.pointA - colliderDistance.pointB);
                }
            }

            Vector3 change = boxCollider.transform.position - posBeforeCollision;
            if (hits != null && hits.Length != 0 && change != Vector3.zero)
            {
                // Is grounded
                if (Vector2.Angle(change, Vector2.up) == 0 && velocity.y < 0)
                {
                    if (!oldGrounded && data.landingSound != "")
                        AudioPlay.PlaySound(data.landingSound, 1);
                    // If grounded
                    Grounded = true;
                }
                else if (Vector2.Angle(change, Vector2.down) == 0 && velocity.y > 0)
                {
                    // If ceiled
                    Ceiled = true;
                }
                else if (Vector2.Angle(change, Vector2.left) == 0 && velocity.x > 0)
                {
                    // If walled
                    velocity.x = data.horizontalBounce * -velocity.x;
                }
                else if (Vector2.Angle(change, Vector2.right) == 0 && velocity.x < 0)
                {
                    // If walled
                    velocity.x = data.horizontalBounce * -velocity.x;
                }
                else
                {
                    // If walled + (grounded or ceiled)
                    velocity.x = data.horizontalBounce * -velocity.x;
                    if (Vector2.Angle(change, Vector2.up) < 5)
                    {
                        Grounded = true;
                    }
                    else
                    {
                        Ceiled = true;
                    }
                }
            }

            ascending = false;
            descending = false;

            if (!grounded)
            {
                ascending = (oldPosition.y < boxCollider.transform.position.y) ? true : false;
                descending = (oldPosition.y > boxCollider.transform.position.y) ? true : false;
            }

            // Collision hit for platform
            foreach (Collider2D hit in hitsPlatform)
            {
                if (hit == boxCollider || !collideWithPlatform)
                    continue;

                BoxCollider2D h = hit as BoxCollider2D;

                if ((boxCollider.transform.position.x + boxCollider.size.x / 2 >
                    hit.transform.position.x - (h.size.x / 2) * h.transform.lossyScale.x ||
                    boxCollider.transform.position.x - boxCollider.size.x / 2 <
                    hit.transform.position.x + (h.size.x / 2) * h.transform.lossyScale.x) &&

                    (oldPosition.y - boxCollider.size.y / 2 >
                    hit.transform.position.y + (h.size.y / 2) * h.transform.lossyScale.y) && descending)
                {
                    ColliderDistance2D colliderDistance = hit.Distance(boxCollider);

                    if (colliderDistance.isOverlapped)
                    {
                        transform.Translate(colliderDistance.pointA - colliderDistance.pointB);

                        if (!oldGrounded && data.landingSound != "")
                            AudioPlay.PlaySound(data.landingSound, 1);

                        Grounded = true;
                        descending = false;
                        ascending = false;
                    }
                }
            }

            oldPosition = boxCollider.transform.position;
            oldGrounded = grounded;

            Cleanup();
        }

        void Cleanup()
        {
            horizontalInput = 0;
            verticalInput = 0;
            jumpInput = false;
        }
    }
}