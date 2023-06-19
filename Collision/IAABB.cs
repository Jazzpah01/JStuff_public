using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAABB
{
    Vector2 MinPosition { get; }
    Vector2 MaxPosition { get; }
    Vector2 Position { get; }
}