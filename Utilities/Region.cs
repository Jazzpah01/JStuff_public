using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JStuff.VectorSwizzling;

namespace JStuff.Utilities
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Region : MonoBehaviour, ISerializationCallbackReceiver
    {
        public UpAxis up = UpAxis.Z;

        public virtual UpAxis Up => up;

        public Vector2 PositionMin
        {
            get
            {
                if (Up == UpAxis.Y)
                {
                    return transform.position.xz() - transform.localScale.xz() / 2;
                }
                else
                {
                    return transform.position.xy() - transform.localScale.xy() / 2;
                }
            }
        }
        public Vector2 Size => transform.localScale;

        public Vector2 PositionMax
        {
            get
            {
                if (Up == UpAxis.Y)
                {
                    return transform.position.xz() + transform.localScale.xz() / 2;
                }
                else
                {
                    return transform.position.xy() + transform.localScale.xy() / 2;
                }
            }
        }

        public Vector2 Range
        {
            get
            {
                if (Up == UpAxis.Y)
                {
                    return transform.localScale.xz();
                }
                else
                {
                    return transform.localScale.xy();
                }
            }
        }


        public virtual void OnAfterDeserialize()
        {
            
        }

        public virtual void OnBeforeSerialize()
        {
            if (Up == UpAxis.Y)
            {
                transform.rotation = Quaternion.Euler(-90, 0, 0);
            }
            else
            {
                transform.rotation = Quaternion.Euler(0, 0, 0);
            }
        }
    }
}