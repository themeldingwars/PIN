using System;

namespace Shared.Udp;

public class ExistsPrefixAttribute : Attribute
{
    public Type ExistsType;
    public object TrueValue;

    public ExistsPrefixAttribute(Type t, object trueVal)
    {
        ExistsType = t;
        TrueValue = trueVal;
    }
}