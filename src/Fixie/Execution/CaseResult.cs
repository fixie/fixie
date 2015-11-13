using System;

namespace Fixie.Execution
{
    [Serializable]
    public class CaseResult
    {
        public CaseResult(PassResult result)
        {
            Status = CaseStatus.Passed;
            Name = result.Name;
            MethodGroup = result.MethodGroup;
            Output = result.Output;
            Duration = result.Duration;
            Exceptions = null;
            SkipReason= null;
        }

        public CaseResult(FailResult result)
        {
            Status = CaseStatus.Failed;
            Name = result.Name;
            MethodGroup = result.MethodGroup;
            Output = result.Output;
            Duration = result.Duration;
            Exceptions = result.Exceptions;
            SkipReason = null;
        }

        public CaseResult(SkipResult result)
        {
            Status = CaseStatus.Skipped;
            Name = result.Name;
            MethodGroup = result.MethodGroup;
            Output = null;
            Duration = TimeSpan.Zero;
            Exceptions = null;
            SkipReason = result.SkipReason;
        }

        public CaseStatus Status { get; }
        public string Name { get; }
        public MethodGroup MethodGroup { get; }
        public string Output { get; }
        public TimeSpan Duration { get; }
        public CompoundException Exceptions { get; }
        public string SkipReason { get; }
    }
}