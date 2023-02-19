using System;

namespace Shared.Udp;

public sealed class PaddingAttribute : Attribute
{
    public PaddingAttribute(int size)
    {
        Size = size;
    }

    public int Size { get; }
}