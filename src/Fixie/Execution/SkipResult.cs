using System;

namespace Fixie.Execution
{
    [Serializable]
    public class SkipResult : CaseResult, Message
    {
        public SkipResult(Case @case, string skipReason)
        {
            Name = @case.Name;
            MethodGroup = @case.MethodGroup;
            SkipReason = skipReason;
        }

        public string Name { get; private set; }
        public MethodGroup MethodGroup { get; private set; }
        public string SkipReason { get; private set; }

        CaseStatus CaseResult.Status { get { return CaseStatus.Skipped; } }
        string CaseResult.Output { get { return null; } }
        TimeSpan CaseResult.Duration { get { return TimeSpan.Zero; } }
        CompoundException CaseResult.Exceptions { get { return null; } }
    }
}