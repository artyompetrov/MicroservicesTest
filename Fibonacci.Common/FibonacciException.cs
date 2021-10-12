using System;

namespace Fibonacci.Common
{
    public class FibonacciException : Exception
    {
        public FibonacciException(string message) : base(message)
        {
        }
    }
}
