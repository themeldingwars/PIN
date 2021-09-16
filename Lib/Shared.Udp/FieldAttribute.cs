using System;
using System.Runtime.CompilerServices;

namespace Shared.Udp
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class FieldAttribute : Attribute
    {
        public FieldAttribute([CallerLineNumber] int order = 0)
        {
            Order = order;
        }

        public int Order { get; }
    }
}