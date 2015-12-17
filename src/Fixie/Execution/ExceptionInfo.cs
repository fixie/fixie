using System;
using Fixie.Internal;

namespace Fixie.Execution
{
    [Serializable]
    public class ExceptionInfo
    {
        public ExceptionInfo(Exception exception, AssertionLibraryFilter filter)
        {
            Type = exception.GetType().FullName;
            IsAssertionException = filter.IsAssertionException(exception);
            Message = exception.Message;
            StackTrace = filter.FilterStackTrace(exception);
            InnerException = exception.InnerException == null ? null : new ExceptionInfo(exception.InnerException, filter);
        }

        public string Type { get; }
        public bool IsAssertionException { get; }
        public string Message { get; }
        public string StackTrace { get; }
        public ExceptionInfo InnerException { get; }
    }
}