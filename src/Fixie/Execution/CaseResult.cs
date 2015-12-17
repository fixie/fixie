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
            Exceptions = null;
            StackTrace = null;
            SkipReason= null;
        }

        public CaseResult(CaseFailed result)
        {
            Status = CaseStatus.Failed;
            Name = result.Name;
            MethodGroup = result.MethodGroup;
            Output = result.Output;
            Duration = result.Duration;
            Exceptions = result.Exceptions;
            StackTrace = result.Exceptions.CompoundStackTrace;
            SkipReason = null;
        }

        public CaseResult(CaseSkipped result)
        {
            Status = CaseStatus.Skipped;
            Name = result.Name;
            MethodGroup = result.MethodGroup;
            Output = null;
            Duration = TimeSpan.Zero;
            Exceptions = null;
            StackTrace = null;
            SkipReason = result.SkipReason;
        }

        public CaseStatus Status { get; }
        public string Name { get; }
        public MethodGroup MethodGroup { get; }
        public string Output { get; }
        public TimeSpan Duration { get; }
        public CompoundException Exceptions { get; }
        public string StackTrace { get; }
        public string SkipReason { get; }
    }
}