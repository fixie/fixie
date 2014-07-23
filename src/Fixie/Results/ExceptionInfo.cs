using System;
using Fixie.Execution;

namespace Fixie.Results
{
    [Serializable]
    public class ExceptionInfo
    {
        public ExceptionInfo(Exception exception, AssertionLibraryFilter filter)
        {
            Type = exception.GetType().FullName;
            DisplayName = filter.DisplayName(exception);
            Message = exception.Message;
            StackTrace = filter.FilterStackTrace(exception);
            InnerException = exception.InnerException == null ? null : new ExceptionInfo(exception.InnerException, filter);
        }

        public string Type { get; private set; }
        public string DisplayName { get; private set; }
        public string Message { get; private set; }
        public string StackTrace { get; private set; }
        public ExceptionInfo InnerException { get; private set; }
    }
}