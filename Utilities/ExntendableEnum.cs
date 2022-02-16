using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnumLike
{
    protected EnumLike()
    {
    }
}

public class TokenTypeC: EnumLike
{
    public static TokenTypeC START = new TokenTypeC();
    public static TokenTypeC END = new TokenTypeC();
    public static TokenTypeC LPAR = new TokenTypeC();
    public static TokenTypeC RPAR = new TokenTypeC();

    protected TokenTypeC() : base()
    {

    }
}