using System;

namespace RecNetLogin
{
    public class RecNetException : Exception
    {
        public RecNetException() { }

        public RecNetException(string message) : base(message) { }

        public RecNetException(string message, Exception innerException) : base(message, innerException) { }
    }
}
