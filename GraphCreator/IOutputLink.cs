using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.GraphCreator
{
    public interface IOutputLink<T> : ILink
    {
        T Evaluate();
    }
    public interface IObserverPort<T> : IOutputLink<T>
    {
        void SubscribePort(OutputFunction<T> function);
    }
    public interface ISimpleNode
    {

    }
    public interface ILink { }

    public interface ICachedLink
    {
        bool ReEvaluate();
    }
}
