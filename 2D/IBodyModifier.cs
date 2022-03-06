using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.TwoD.Platformer
{
    public interface IBodyModifier : IComparable
    {
        int Precedence { get; }

        void ApplyFilter(Body2d body);
        void ApplyIvertedFilter(Body2d body);
    }
}