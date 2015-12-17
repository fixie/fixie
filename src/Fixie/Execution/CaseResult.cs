using System;

namespace Fixie.Execution
{
    [Serializable]
    public class CaseResult : IMessage
    {
        public CaseResult(CasePassed result)
        {
            Status = CaseStatus.Passed;
            Name = result.Name;
            MethodGroup = result.MethodGroup;
            Output = result.Output;
            Duration = result.Duration;
            ExceptionSummary = null;
            ExceptionType = null;
            StackTrace = null;
            Message = null;
        }

        public CaseResult(CaseFailed result)
        {
            Status = CaseStatus.Failed;
            Name = result.Name;
            MethodGroup = result.MethodGroup;
            Output = result.Output;
            Duration = result.Duration;
            ExceptionSummary = result.Exceptions.PrimaryException.DisplayName;
            ExceptionType = result.Exceptions.PrimaryException.Type;
            StackTrace = result.Exceptions.CompoundStackTrace;
            Message = result.Exceptions.PrimaryException.Message;
        }

        public CaseResult(CaseSkipped result)
        {
            Status = CaseStatus.Skipped;
            Name = result.Name;
            MethodGroup = result.MethodGroup;
            Output = null;
            Duration = TimeSpan.Zero;
            ExceptionSummary = null;
            ExceptionType = null;
            StackTrace = null;
            Message = result.SkipReason;
        }

        public CaseStatus Status { get; }
        public string Name { get; }
        public MethodGroup MethodGroup { get; }
        public string Output { get; }
        public TimeSpan Duration { get; }

        public string ExceptionSummary { get; }
        public string ExceptionType { get; }
        public string StackTrace { get; }
        public string Message { get; }
    }
}