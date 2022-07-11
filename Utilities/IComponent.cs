using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.Utilities
{
    public interface IComponent
    {
        GameObject gameObject { get; }
        Transform transform { get; }
        T GetComponent<T>();
        T[] GetComponents<T>();
        T GetComponentInChildren<T>();
        T[] GetComponentsInChildren<T>();
    }
}