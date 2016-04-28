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

        public MethodGroup MethodGroup { get; private set; }
        public string Name { get; private set; }
        CaseStatus CaseCompleted.Status { get { return CaseStatus.Failed; } }
        public TimeSpan Duration { get; private set; }
        public string Output { get; private set; }
        public CompoundException Exceptions { get; private set; }
        string CaseCompleted.SkipReason { get { return null; } }
    }
}