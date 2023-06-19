using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Flags]
public enum EdgeOptions
{
    None = 0b0,
    UseDiagonals = 0b1,
    MultiplyDistance = 0b10,
    AddDistance = 0b100
}