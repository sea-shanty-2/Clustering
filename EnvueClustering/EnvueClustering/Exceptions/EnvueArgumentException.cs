using System;

namespace EnvueClustering.Exceptions
{
    public class EnvueArgumentException : EnvueException
    {
        public EnvueArgumentException(){}
        public EnvueArgumentException(string message) : base(message){}
        public EnvueArgumentException(string message, Exception innerException) : base(message, innerException){}
    }
}