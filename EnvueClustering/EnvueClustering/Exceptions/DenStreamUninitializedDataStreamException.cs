namespace EnvueClustering.Exceptions
{
    public class DenStreamUninitializedDataStreamException : EnvueException
    {
        public DenStreamUninitializedDataStreamException(string message, EnvueException innerException) : base(message,
            innerException)
        {
        }

        public DenStreamUninitializedDataStreamException(string message) : base(message)
        {
        }
    }
}