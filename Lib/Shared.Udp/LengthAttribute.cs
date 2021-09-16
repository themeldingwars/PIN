using System;

namespace Shared.Udp
{
    public sealed class LengthAttribute : Attribute
    {
        public LengthAttribute(int len)
        {
            Length = len;
        }

        public int Length { get; }
    }
}