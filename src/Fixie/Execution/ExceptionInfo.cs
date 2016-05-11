using System;
using Fixie.Internal;

namespace Fixie.Execution
{
    [Serializable]
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
        public string Type { get; }

        [Obsolete]
        public string Message { get; }

        [Obsolete]
        public string StackTrace { get; }

        [Obsolete]
        public ExceptionInfo InnerException { get; }
    }
}