using System;

namespace Shared.Udp;

public sealed class LengthPrefixedAttribute : Attribute
{
    public LengthPrefixedAttribute(Type t)
    {
        LengthType = t;
    }
    
    public Type LengthType { get; }
}