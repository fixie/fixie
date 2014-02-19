using System;

namespace Fixie.Results
{
    [Serializable]
    public class ExceptionInfo
    {
        public ExceptionInfo(Exception exception)
        {
            Type = exception.GetType().FullName;
            Message = exception.Message;
            StackTrace = exception.StackTrace;
            InnerException = exception.InnerException == null ? null : new ExceptionInfo(exception.InnerException);
        }

        public string Type { get; private set; }
        public string Message { get; private set; }
        public string StackTrace { get; private set; }
        public ExceptionInfo InnerException { get; private set; }
    }
}