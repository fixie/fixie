using System;

namespace Fixie.Execution
{
    [Serializable]
    public class CaseSkipped : CaseCompleted, Message
    {
        public CaseSkipped(Case @case, string skipReason)
        {
            MethodGroup = @case.MethodGroup;
            Name = @case.Name;
            Status = CaseStatus.Skipped;
            Duration = TimeSpan.Zero;
            Output = null;
            Exceptions = null;
            SkipReason = skipReason;
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