using System;

namespace Shared.Udp;

public sealed class LengthPrefixedAttribute : Attribute
{
    public readonly Type LengthType;

    public LengthPrefixedAttribute(Type t)
    {
        LengthType = t;
    }
}