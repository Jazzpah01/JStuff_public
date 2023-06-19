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

        public virtual void Start()
        {
            Destroy(GetComponent<SpriteRenderer>());
        }


        public Vector2 MinPosition
        {
            get
            {
                if (Up == UpAxis.Y)
                {
                    return transform.position.xz() - Range / 2;
                }
                else
                {
                    return transform.position.xy() - Range / 2;
                }
            }
        }
        public Vector2 Size => transform.localScale;

        public Vector2 MaxPosition
        {
            get
            {
                if (Up == UpAxis.Y)
                {
                    return transform.position.xz() + Range / 2;
                }
                else
                {
                    return transform.position.xy() + Range / 2;
                }
            }
        }

        public Vector2 Range
        {
            get
            {
                return new Vector2(Mathf.Abs(transform.localScale.x), Mathf.Abs(transform.localScale.y));
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