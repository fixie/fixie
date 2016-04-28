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

        public MethodGroup MethodGroup { get; private set; }
        public string Name { get; private set; }
        CaseStatus CaseCompleted.Status { get { return CaseStatus.Skipped; } }
        TimeSpan CaseCompleted.Duration { get { return TimeSpan.Zero; } }
        string CaseCompleted.Output { get { return null; } }
        CompoundException CaseCompleted.Exceptions { get { return null; } }
        public string SkipReason { get; private set; }
    }
}