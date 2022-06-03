using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.Threading
{
    [CreateAssetMenu(menuName = "JStuff/JobManagerData")]
    public class JobManagerData : ScriptableObject
    {
        [Min(1)] [SerializeField] public int maxJobs = 1000;
        [Min(1)] [SerializeField] public int threadAmount = 1;
        [SerializeField] public bool setThreads = false;
        [SerializeField] public List<DedicatedThreads> dedicatedThreads = null;
        [SerializeField] public bool dedicatedThreadsEnabled = false;
        [Min(1)] [SerializeField] public int numberOfPriorities = 1;
        [SerializeField] public bool advancedOptions = false;
        [SerializeField] public bool allowMultithreading = true;
        [SerializeField] public bool consumeAsStartCoroutine = false;
        [SerializeField] public OverloadProtocol overloadProtocol = 0;
        [SerializeField] public UpdateProtocol updateProtocol = 0;
    }
}