using System;
using Fixie.Discovery;
using Fixie.Results;

namespace Fixie.Execution
{
    [Serializable]
    public class SkipResult : CaseResult
    {
        public SkipResult(Case @case, string skipReason)
        {
            Name = @case.Name;
            MethodGroup = @case.MethodGroup;
            Output = @case.Output;
            Duration = @case.Duration;

            SkipReason = skipReason;
        }

        CaseStatus CaseResult.Status { get { return CaseStatus.Skipped; } }

        public string Name { get; private set; }
        public MethodGroup MethodGroup { get; private set; }
        public string Output { get; private set; }
        public TimeSpan Duration { get; private set; }

        CompoundException CaseResult.Exceptions { get { return null; } }
        public string SkipReason { get; private set; }
    }
}