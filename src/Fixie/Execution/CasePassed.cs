namespace Fixie.Execution
{
    using System;

    [Serializable]
    public class CasePassed : CaseCompleted
    {
        public CasePassed(Case @case)
        {
            Name = @case.Name;
            MethodGroup = @case.MethodGroup;
            Output = @case.Output;
            Duration = @case.Duration;
        }

        public string Name { get; private set; }
        public MethodGroup MethodGroup { get; private set; }
        public string Output { get; private set; }
        public TimeSpan Duration { get; private set; }

        CaseStatus CaseCompleted.Status { get { return CaseStatus.Passed; } }
        CompoundException CaseCompleted.Exceptions { get { return null; } }
        string CaseCompleted.SkipReason { get { return null; } }
    }
}