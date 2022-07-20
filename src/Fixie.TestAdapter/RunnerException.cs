namespace Fixie.TestAdapter
{
    using System;
    using System.Runtime.Serialization;
    using System.Text;
    using Internal;

    [Serializable]
    public class RunnerException : Exception
    {
        public RunnerException(Exception exception)
            : base(exception.ToString())
        {
        }

        public RunnerException(PipeMessage.Exception exception)
            : base(new StringBuilder()
                .AppendLine()
                .AppendLine()
                .AppendLine(exception.Message)
                .AppendLine()
                .AppendLine(exception.Type)
                .AppendLine(exception.StackTrace)
                .ToString())
        {
        }

        public RunnerException() : base()
        {
        }

        public RunnerException(string? message) : base(message)
        {
        }

        public RunnerException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        protected RunnerException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            :base(serializationInfo, streamingContext)
        {
        }
    }
}