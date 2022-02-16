using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.Collection
{
    public interface ICondition<T>
    {
        bool IsTrue(T condition);
    }
}