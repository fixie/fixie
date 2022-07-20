namespace Fixie.Tests.Internal
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class StubException : Exception
    {
        public StubException(string message)
            : base(message) { }

        public StubException() : base()
        {
        }

        public StubException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        public override string StackTrace
            => "<<Stack Trace>>";

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        protected StubException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}