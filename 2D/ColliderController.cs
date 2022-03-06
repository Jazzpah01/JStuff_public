using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.TwoD.Platformer
{
    public static class ColliderController
    {
        private static bool HasColliderFlag(Collider2D collier, string f)
        {
            GameObject go = collier.gameObject;
            CustomFlags co = go.GetComponent<CustomFlags>();

            if (co == null)
            {
                return false;
            }

            return co.HasFlag(f);
        }

        public static Collider2D[] OverlapBoxAllWithFlag(Vector2 point, Vector2 size, string flag)
        {
            Collider2D[] hits = Physics2D.OverlapBoxAll(point, size, 0);
            List<Collider2D> filter = new List<Collider2D>();
            foreach (Collider2D hit in hits)
            {
                if (HasColliderFlag(hit, flag))
                    filter.Add(hit);
            }
            return filter.ToArray();
        }
        public static Collider2D[] OverlapBoxAllWithFlags(Vector2 point, Vector2 size, List<string> flags)
        {
            Collider2D[] hits = Physics2D.OverlapBoxAll(point, size, 0);
            List<Collider2D> filter = new List<Collider2D>();
            foreach (Collider2D hit in hits)
            {
                foreach (string s in flags)
                {
                    if (HasColliderFlag(hit, s))
                        filter.Add(hit);
                }
            }
            return filter.ToArray();
        }
    }




    /*
     * 
     * 
     * 
    public class ColliderController : MonoBehaviour
    {
        [SerializeField]
        public ColliderFlags flags;

        public enum Flag
        {
            Solid, Flower, Body, Unit, Player, Enemy, Projectile, Platform, Flammable
        }

        private static bool HasColliderFlag(Collider2D collier, Flag f)
        {
            GameObject go = collier.gameObject;
            ColliderController co = go.GetComponent<ColliderController>();

            if (co == null || co.flags == null)
                return false;//throw new System.Exception("GameObject of collider doesn't have ColliderController!");

            switch (f)
            {
                case Flag.Solid:
                    return co.flags.Solid;
                    break;
                case Flag.Player:
                    return co.flags.Player;
                    break;
                case Flag.Flower:
                    return co.flags.Flower;
                    break;
                case Flag.Unit:
                    return co.flags.Unit;
                case Flag.Platform:
                    return co.flags.Platform;
                case Flag.Enemy:
                    return co.flags.Enemy;
                case Flag.Flammable:
                    return co.flags.Flammable;
                default:
                    throw new System.Exception("Flag not implemented: " + f.ToString());
            }
        }

        public static Collider2D[] OverlapBoxAllWithFlag(Vector2 point, Vector2 size, Flag flag)
        {
            Collider2D[] hits = Physics2D.OverlapBoxAll(point, size, 0);
            List<Collider2D> filter = new List<Collider2D>();
            foreach (Collider2D hit in hits){
                if (HasColliderFlag(hit, flag))
                    filter.Add(hit);
            }
            return filter.ToArray();
        }
    }
     */
}