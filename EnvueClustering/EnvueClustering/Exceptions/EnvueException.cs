using System;

namespace EnvueClustering.Exceptions
{
    public class EnvueException : Exception
    {
        public EnvueException(){}
        public EnvueException(string message) : base(message){}
        public EnvueException(string message, Exception innerException) : base(message, innerException){}
    }
}