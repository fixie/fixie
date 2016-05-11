using System;
using Fixie.Internal;

namespace Fixie.Execution
{
    [Serializable]
    [Obsolete]
    public class ExceptionInfo
    {
        public ExceptionInfo(Exception exception)
        {
            Message = exception.Message;
            InnerException = exception.InnerException == null ? null : new ExceptionInfo(exception.InnerException);
        }

        [Obsolete]
        public string Message { get; }

        [Obsolete]
        public ExceptionInfo InnerException { get; }
    }
}