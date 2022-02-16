using JStuff.GraphCreator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILinkable
{
    ILink LinkedPort { get; }
    void LinkPort(Link outputPort);
    void RemoveLink();
}
public delegate T OutputFunction<T>();