using UnityEngine;

namespace JStuff.Threading
{
    [System.Serializable]
    public class DedicatedThreads
    {
        [Min(1)] public int numberOfThreads;
        [Min(0)] public int priority;
    }
}