using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.Collections
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ConditionalStack<T>
    {
        List<T> list;

        public ConditionalStack()
        {
            list = new List<T>();
        }

        public T Pop(ICondition<T> conditition)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (conditition.IsTrue(list[i]))
                {
                    T elm = list[i];
                    list.Remove(elm);
                    return elm;
                }
            }
            throw new System.Exception("List does not contain an element thet fulfills the condition.");
        }

        public bool Contains(T elm)
        {
            return list.Contains(elm);
        }

        public bool Contains(ICondition<T> conditition)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (conditition.IsTrue(list[i]))
                {
                    return true;
                }
            }
            return false;
        }
    }
}