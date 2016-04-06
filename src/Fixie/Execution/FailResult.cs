using System;
using Fixie.Internal;

namespace Fixie.Execution
{
    [Serializable]
    public class FailResult : CaseResult, Message
    {
        public FailResult(Case @case, AssertionLibraryFilter filter)
        {
            Name = @case.Name;
            MethodGroup = @case.MethodGroup;
            Output = @case.Output;
            Duration = @case.Duration;

            Exceptions = new CompoundException(@case.Exceptions, filter);
        }

        public string Name { get; private set; }
        public MethodGroup MethodGroup { get; private set; }
        public string Output { get; private set; }
        public TimeSpan Duration { get; private set; }
        public CompoundException Exceptions { get; private set; }

        CaseStatus CaseResult.Status { get { return CaseStatus.Failed; } }
        string CaseResult.SkipReason { get { return null; } }
    }
}