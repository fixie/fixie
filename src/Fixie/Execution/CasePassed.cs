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

        public MethodGroup MethodGroup { get; private set; }
        public string Name { get; private set; }
        CaseStatus CaseCompleted.Status { get { return CaseStatus.Passed; } }
        public TimeSpan Duration { get; private set; }
        public string Output { get; private set; }
        CompoundException CaseCompleted.Exceptions { get { return null; } }
        string CaseCompleted.SkipReason { get { return null; } }
    }
}