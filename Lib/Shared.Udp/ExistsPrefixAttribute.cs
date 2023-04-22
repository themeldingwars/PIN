using System;

namespace Shared.Udp;

public class ExistsPrefixAttribute : Attribute
{
    public readonly Type ExistsType;
    public readonly object TrueValue;

    public ExistsPrefixAttribute(Type t, object trueVal)
    {
        ExistsType = t;
        TrueValue = trueVal;
    }
}