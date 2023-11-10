using System;

namespace Shared.Udp;

public class ExistsPrefixAttribute : Attribute
{
    public ExistsPrefixAttribute(Type t, object trueVal)
    {
        ExistsType = t;
        TrueValue = trueVal;
    }
    
    public Type ExistsType { get; }
    public object TrueValue { get; }
}