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

        public CaseStatus Status { get; private set; }
        public string Name { get; private set; }
        public MethodGroup MethodGroup { get; private set; }
        public string Output { get; private set; }
        public TimeSpan Duration { get; private set; }
        public CompoundException Exceptions { get; private set; }
        public string SkipReason { get; private set; }
    }
}