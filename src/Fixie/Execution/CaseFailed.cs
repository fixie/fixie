using System;
using Fixie.Internal;

namespace Fixie.Execution
{
    [Serializable]
    public class CaseFailed : CaseCompleted, Message
    {
        public CaseFailed(Case @case, AssertionLibraryFilter filter)
        {
            MethodGroup = @case.MethodGroup;
            Name = @case.Name;
            Status = CaseStatus.Failed;
            Duration = @case.Duration;
            Output = @case.Output;
            Exceptions = new CompoundException(@case.Exceptions, filter);
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