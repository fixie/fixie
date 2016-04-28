using System;

namespace Fixie.Execution
{
    [Serializable]
    public class CasePassed : CaseCompleted, Message
    {
        public CasePassed(Case @case)
        {
            MethodGroup = @case.MethodGroup;
            Name = @case.Name;
            Status = CaseStatus.Passed;
            Duration = @case.Duration;
            Output = @case.Output;
            Exceptions = null;
            SkipReason = null;
        }

        public MethodGroup MethodGroup { get; }
        public string Name { get; }
        public CaseStatus Status { get; }
        public TimeSpan Duration { get; }
        public string Output { get; }
        public CompoundException Exceptions { get; }
        public string SkipReason { get; }
    }
}