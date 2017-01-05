namespace Fixie.Execution
{
    using System;

    [Obsolete]
    public class ExceptionInfo
    {
        public ExceptionInfo(Exception exception)
        {
            Message = exception.Message;
            InnerException = exception.InnerException == null ? null : new ExceptionInfo(exception.InnerException);
        }

        [Obsolete]
        public string Message { get; private set; }
        [Obsolete]
        public ExceptionInfo InnerException { get; private set; }
    }
}