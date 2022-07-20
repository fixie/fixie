namespace Fixie.Console
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class CommandLineException : Exception
    {
        public CommandLineException(string message, Exception innerException)
            : base(message, innerException) { }

        public CommandLineException(string message)
            : base(message) { }

        protected CommandLineException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            :base(serializationInfo, streamingContext)
        {
        }

        public CommandLineException() : base()
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
