using System;
using Fixie.Results;

namespace Fixie.Execution
{
    public class FailResult
    {
        public FailResult(CaseExecution execution, AssertionLibraryFilter filter)
        {
            Case = execution.Case;
            Output = execution.Output;
            Duration = execution.Duration;
            Exceptions = new CompoundException(execution.Exceptions, filter);
        }

        public Case Case { get; private set; }
        public string Output { get; private set; }
        public TimeSpan Duration { get; private set; }
        public CompoundException Exceptions { get; private set; }
    }
}