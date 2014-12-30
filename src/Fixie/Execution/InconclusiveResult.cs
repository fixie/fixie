using System;
using System.Linq;
using Fixie.Internal;

namespace Fixie.Execution
{
    [Serializable]
    public class InconclusiveResult : CaseResult
    {
        public InconclusiveResult(Case @case, AssertionLibraryFilter filter)
        {
            Name = @case.Name;
            MethodGroup = @case.MethodGroup;
            Output = @case.Output;
            Duration = @case.Duration;

            Exceptions =
                @case.Exceptions.Any()
                    ? new CompoundException(@case.Exceptions, filter)
                    : null;
        }

        public string Name { get; private set; }
        public MethodGroup MethodGroup { get; private set; }
        public string Output { get; private set; }
        public TimeSpan Duration { get; private set; }
        public CompoundException Exceptions { get; private set; }

        CaseStatus CaseResult.Status { get { return CaseStatus.Inconclusive; } }
        string CaseResult.SkipReason { get { return null; } }
    }
}