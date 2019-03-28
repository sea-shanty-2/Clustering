namespace EnvueClustering.Exceptions
{
    public class DenStreamException : EnvueException
    {
        public DenStreamException(string message, EnvueException innerException) : base(message, innerException)
        {
        }

        public DenStreamException(string message) : base(message)
        {
        }
    }
}