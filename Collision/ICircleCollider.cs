using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICircleCollider : IAABB
{
    Vector2 Position { get; }
    float Radius { get; }
}