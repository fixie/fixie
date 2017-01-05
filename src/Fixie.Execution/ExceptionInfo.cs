namespace Fixie.Execution
{
    using System;

    [Obsolete]
    public class ExceptionInfo
    {
        public ExceptionInfo(Exception exception, AssertionLibraryFilter filter)
        {
            Type = exception.GetType().FullName;
            Message = exception.Message;
            StackTrace = filter.FilterStackTrace(exception);
            InnerException = exception.InnerException == null ? null : new ExceptionInfo(exception.InnerException, filter);
        }

        [Obsolete]
        public string Type { get; private set; }
        [Obsolete]
        public string Message { get; private set; }
        [Obsolete]
        public string StackTrace { get; private set; }
        [Obsolete]
        public ExceptionInfo InnerException { get; private set; }
    }
}