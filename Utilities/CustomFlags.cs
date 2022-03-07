using JStuff.TwoD.Platformer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomFlags : MonoBehaviour
{
    [SerializeField] private List<ColliderFlag> flags;
    private int _flags;

    public int Flags => _flags;

    private void Awake()
    {
        _flags = ColliderController.GetFlags(flags);
    }
}