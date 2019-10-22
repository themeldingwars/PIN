using System;

namespace Operator.Infrastructure.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string message)
            : base(message)
        {
        }
    }
}
