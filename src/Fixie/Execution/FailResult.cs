using System;
using Fixie.Discovery;
using Fixie.Results;

namespace Fixie.Execution
{
    [Serializable]
    public class FailResult : CaseResult
    {
        public FailResult(Case @case, AssertionLibraryFilter filter)
        {
            Name = @case.Name;
            MethodGroup = @case.MethodGroup;
            Output = @case.Output;
            Duration = @case.Duration;

            Exceptions = new CompoundException(@case.Exceptions, filter);
        }

        CaseStatus CaseResult.Status { get { return CaseStatus.Failed; } }

        public string Name { get; private set; }
        public MethodGroup MethodGroup { get; private set; }
        public string Output { get; private set; }
        public TimeSpan Duration { get; private set; }

        public CompoundException Exceptions { get; private set; }
        string CaseResult.SkipReason { get { return null; } }
    }
}