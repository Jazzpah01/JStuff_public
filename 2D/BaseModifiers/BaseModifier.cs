using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.TwoD.Platformer
{
    [CreateAssetMenu(menuName = "JStuff/2D/Base Modifier")]
    public class BaseModifier : ScriptableObject, IBodyModifier
    {
        [Header("Physics variables multipliers")]
        [SerializeField, Tooltip("Max speed, in units per second, that the character moves.")]
        public float speed = 1;

        [SerializeField, Tooltip("Acceleration while grounded.")]
        public float walkAcceleration = 1;

        [SerializeField, Tooltip("Acceleration while in the air.")]
        public float airAcceleration = 1;

        [SerializeField, Tooltip("Deceleration applied when character is grounded and not attempting to move.")]
        public float groundDeceleration = 1;

        [SerializeField, Tooltip("Max height the character will jump regardless of gravity")]
        public float jumpHeight = 1;

        [SerializeField]
        public float gravityAcceleration = 1;

        [Header("Advanced physics multipliers")]
        [SerializeField, Tooltip("Gravitymultiplier on descend.")]
        public float GMDescend = 1;
        [SerializeField, Tooltip("Gravity multiplier on FallInput().")]
        public float GMFall = 1;
        [SerializeField, Tooltip("When hitting a wall, apply the inverse velocity x multiplied with the bounce factor.")]
        public float horizontalBounce = 1;
        [SerializeField, Tooltip("When hitting the ground, apply the inverse velocity y multiplied with the bounce factor.")]
        public float verticalBounce = 1;

        public enum Pivot
        {
            Up,
            Down,
            Left,
            Right,
            Center
        }

        public Pivot pivot;

        public float colliderXMultiplier = 1;
        public float colliderYMultiplier = 1;

        public int Precedence => 100;

        public void ApplyFilter(Body2d body)
        {
            body.data.speed *= speed;
            body.data.walkAcceleration *= walkAcceleration;
            body.data.airAcceleration *= airAcceleration;
            body.data.groundDeceleration *= groundDeceleration;
            body.data.jumpHeight *= jumpHeight;
            body.data.gravityAcceleration *= gravityAcceleration;

            body.data.horizontalBounce *= horizontalBounce;
            body.data.verticalBounce *= verticalBounce;

            BoxCollider2D collider = body.GetComponent<BoxCollider2D>();
            Vector2 position = body.transform.position;
            switch (pivot)
            {
                case Pivot.Down:
                    position.y -= (collider.size.y - collider.size.y * colliderYMultiplier) / 2;
                    break;
                case Pivot.Up:
                    position.y += (collider.size.y - collider.size.y * colliderYMultiplier) / 2;
                    break;
                case Pivot.Left:
                    position.x -= (collider.size.x - collider.size.x * colliderXMultiplier) / 2;
                    break;
                case Pivot.Right:
                    position.x += (collider.size.x - collider.size.x * colliderXMultiplier) / 2;
                    break;
            }
            body.transform.position = position;

            Vector2 size = collider.size;
            size.x *= colliderXMultiplier;
            size.y *= colliderYMultiplier;
            collider.size = size;
        }

        public void ApplyIvertedFilter(Body2d body)
        {
            BoxCollider2D collider = body.GetComponent<BoxCollider2D>();
            Vector2 position = body.transform.position;
            switch (pivot)
            {
                case Pivot.Down:
                    position.y += (collider.size.y - collider.size.y * colliderYMultiplier);
                    break;
                case Pivot.Up:
                    position.y -= (collider.size.y - collider.size.y * colliderYMultiplier);
                    break;
                case Pivot.Left:
                    position.x += (collider.size.x - collider.size.x * colliderXMultiplier);
                    break;
                case Pivot.Right:
                    position.x -= (collider.size.x - collider.size.x * colliderXMultiplier);
                    break;
            }
            body.transform.position = position;

            Vector2 size = collider.size;
            size.x /= colliderXMultiplier;
            size.y /= colliderYMultiplier;
            collider.size = size;
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
                throw new System.Exception();

            IBodyModifier filter = (IBodyModifier)obj;

            if (filter.Precedence < this.Precedence)
            {
                return 1;
            }
            else if (filter.Precedence == this.Precedence)
            {
                return 0;
            }
            else
            {
                return -1;
            }
        }
    }
}