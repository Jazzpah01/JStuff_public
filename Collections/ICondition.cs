using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.Collections
{
    public interface ICondition<T>
    {
        bool IsTrue(T condition);
    }
}