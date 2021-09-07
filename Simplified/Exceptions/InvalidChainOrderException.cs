using System;

namespace Simplified.Exceptions
{
    class InvalidChainOrderException : Exception
    {
        public InvalidChainOrderException() : base("Order is -> ExecStoredProcedure -> Apply -> Map -> Output") { }
    }
}
