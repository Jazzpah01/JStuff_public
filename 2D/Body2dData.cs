using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.TwoD.Platformer
{
    [CreateAssetMenu(menuName = "JStuff/2D/Body Data")]

    public class Body2dData : ScriptableObject
    {
        [Header("Flags and stuff")]
        [SerializeField, Tooltip("Collider flags that should collide with this.")]
        public List<ColliderFlag> ColliderFlags;
        [SerializeField, Tooltip("Collider flags that should collide with this as a platform.")]
        public List<ColliderFlag> PlatformFlags;

        [Header("Physics variables")]
        [SerializeField, Tooltip("Max speed, in units per second, that the character moves.")]
        public float speed = 9;

        [SerializeField, Tooltip("Acceleration while grounded.")]
        public float walkAcceleration = 75;

        [SerializeField, Tooltip("Acceleration while in the air.")]
        public float airAcceleration = 30;

        [SerializeField, Tooltip("Deceleration applied when character is grounded and not attempting to move.")]
        public float groundDeceleration = 70;

        [SerializeField, Tooltip("Max height the character will jump regardless of gravity")]
        public float jumpHeight = 4;

        [SerializeField]
        public float gravityAcceleration = -9.81f;

        public float initialGravityAcceleration;

        [Header("Advanced physics")]
        [SerializeField, Tooltip("Gravitymultiplier on descend.")]
        public float GMDescend = 1;
        [SerializeField, Tooltip("Gravity multiplier on FallInput().")]
        public float GMFall = 1;
        [SerializeField, Tooltip("Instant drop on FallInput().")]
        public bool dropOnFall = true;
        [SerializeField, Tooltip("Call FallInput() when descending.")]
        public bool fallOnDescend = true;
        [SerializeField, Tooltip("When hitting a wall, apply the inverse velocity x multiplied with the bounce factor.")]
        public float horizontalBounce;
        [SerializeField, Tooltip("When hitting the ground, apply the inverse velocity y multiplied with the bounce factor.")]
        public float verticalBounce;
        [SerializeField, Tooltip("The amount of frames where you can still jump after leaving ground.")]
        public int jackalFrames; // Also known as Coyote time
                                 //[SerializeField, Tooltip("???.")]
                                 //public bool fall;
        [SerializeField, Tooltip("The amount of frames jump input will be buffered.")]
        public int inputBufferJump = 1;


        [Header("Other")]
        [SerializeField]
        public bool groundOnSpawn = false;
        [SerializeField]
        public string landingSound;
    }
}