using System;

[Flags]
public enum Connections
{
    none = 0b0000,
    right = 0b0001,
    up = 0b0010,
    left = 0b0100,
    down = 0b1000
}

public enum MaxConnections
{
    none = 0,
    one = 1,
    two = 2,
    three = 3,
    four = 4
}

//public static class ConnectionsExtensions
//{
//public static bool HasFlag(this Connections connection, Connections flag)
//{
//    return (connection & flag) != 0;
//}
//public static bool HasFlags(this Connections connection, Connections flags)
//{
//    return (connection & flags) != flags;
//}
//public static bool HasFlags(this Connections connection, params Connections[] flags)
//{
//    foreach (Connections flag in flags)
//    {
//        if ((connection & flag) == 0)
//            return false;
//    }

//    return true;
//}
//}