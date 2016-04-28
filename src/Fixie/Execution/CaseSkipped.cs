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
            SkipReason = skipReason;
        }

        public MethodGroup MethodGroup { get; }
        public string Name { get; }
        public CaseStatus Status { get { return CaseStatus.Skipped; } }
        public TimeSpan Duration { get { return TimeSpan.Zero; } }
        public string Output { get { return null; } }
        public CompoundException Exceptions { get { return null; } }
        public string SkipReason { get; }
    }
}