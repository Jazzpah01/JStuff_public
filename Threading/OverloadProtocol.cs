using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.Threading
{
    public enum OverloadProtocol
    {
        Stall = 0,
        DropAndCallJobFailed,
        DropAndThrowException,
        ReplaceAJobAndCallJobFailed,
        FinishJobs
    }
}