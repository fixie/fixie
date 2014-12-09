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

        public string Name { get; private set; }
        public MethodGroup MethodGroup { get; private set; }
        public string Output { get; private set; }
        public TimeSpan Duration { get; private set; }

        CaseStatus CaseResult.Status { get { return CaseStatus.Passed; } }
        CompoundException CaseResult.Exceptions { get { return null; } }
        string CaseResult.SkipReason { get { return null; } }
    }
}