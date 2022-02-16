using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stretchable : MonoBehaviour
{
    public enum Axis
    {
        y, z
    }

    public Axis positionAxis;

    public Vector2 Position
    {
        get {
            if (positionAxis == Axis.y)
            {
                return transform.position - transform.localScale / 2;
            } else
            {
                return new Vector2(transform.position.x, transform.position.z) - (Vector2)transform.localScale / 2;
            }
        }
    }
    public Vector2 Size => transform.localScale;
}