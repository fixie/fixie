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
            Duration = @case.Duration;
            Output = @case.Output;

            Exceptions = new CompoundException(@case.Exceptions, filter);
        }

        public MethodGroup MethodGroup { get; }
        public string Name { get; }
        public CaseStatus Status { get { return CaseStatus.Failed; } }
        public TimeSpan Duration { get; }
        public string Output { get; }
        public CompoundException Exceptions { get; }
        public string SkipReason { get { return null; } }
    }
}