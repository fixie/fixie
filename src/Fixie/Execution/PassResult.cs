using System;
using Fixie.Discovery;
using Fixie.Results;

namespace Fixie.Execution
{
    [Serializable]
    public class PassResult : CaseResult
    {
        public PassResult(Case @case)
        {
            Name = @case.Name;
            MethodGroup = @case.MethodGroup;
            Output = @case.Output;
            Duration = @case.Duration;
        }

        CaseStatus CaseResult.Status { get { return CaseStatus.Passed; } }

        public string Name { get; private set; }
        public MethodGroup MethodGroup { get; private set; }
        public string Output { get; private set; }
        public TimeSpan Duration { get; private set; }

        CompoundException CaseResult.Exceptions { get { return null; } }
        string CaseResult.SkipReason { get { return null; } }
    }
}