using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Shared.Udp {
    [AttributeUsage( AttributeTargets.Field, Inherited = true, AllowMultiple = false )]
    public sealed class FieldAttribute : Attribute {
        private readonly int order_;
        public FieldAttribute( [CallerLineNumber] int order = 0 ) {
            order_ = order;
        }

        public int Order { get { return order_; } }
    }
}
