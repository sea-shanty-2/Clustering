namespace EnvueClustering.Exceptions
{
    public class DenStreamNullReferenceException : DenStreamException
    {
        public DenStreamNullReferenceException(string message, DenStreamException innerException) : base(message,
            innerException)
        {
        }

        public DenStreamNullReferenceException(string message) : base(message)
        {
        }
    }
}