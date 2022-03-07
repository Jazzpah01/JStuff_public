using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.TwoD.Platformer
{
    public static class ColliderController
    {
        private static bool HasColliderFlag(Collider2D collier, int f)
        {
            GameObject go = collier.gameObject;
            CustomFlags co = go.GetComponent<CustomFlags>();

            if (co == null)
            {
                return false;
            }

            return ((f & co.Flags) != 0);
        }

        public static Collider2D[] OverlapBoxAllWithFlags(Vector2 point, Vector2 size, int flags)
        {
            Collider2D[] hits = Physics2D.OverlapBoxAll(point, size, 0);
            List<Collider2D> filter = new List<Collider2D>();
            foreach (Collider2D hit in hits)
            {
                if (HasColliderFlag(hit, flags))
                    filter.Add(hit);
            }
            return filter.ToArray();
        }

        public static int GetFlags(List<ColliderFlag> colliderFlags)
        {
            int retval = 0;
            foreach (ColliderFlag item in colliderFlags)
            {
                retval = retval | 0b1 << item.uniqueIndex;
            }
            return retval;
        }
    }
}