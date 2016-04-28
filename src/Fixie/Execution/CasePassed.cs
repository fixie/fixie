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
            Duration = @case.Duration;
            Output = @case.Output;
        }

        public MethodGroup MethodGroup { get; }
        public string Name { get; }
        public CaseStatus Status { get { return CaseStatus.Passed; } }
        public TimeSpan Duration { get; }
        public string Output { get; }
        public CompoundException Exceptions { get { return null; } }
        public string SkipReason { get { return null; } }
    }
}