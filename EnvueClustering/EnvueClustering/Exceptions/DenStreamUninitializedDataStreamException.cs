namespace EnvueClustering.Exceptions
{
    public class DenStreamUninitializedDataStreamException : DenStreamException
    {
        public DenStreamUninitializedDataStreamException(string message, DenStreamException innerException) : base(message,
            innerException)
        {
        }

        public DenStreamUninitializedDataStreamException(string message) : base(message)
        {
        }
    }
}