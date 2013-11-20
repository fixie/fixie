using System;

namespace Fixie
{
    public class PassResult
    {
        public PassResult(CaseExecution execution)
        {
            Case = execution.Case;
            Output = execution.Output;
            Duration = execution.Duration;
        }

        public Case Case { get; private set; }
        public string Output { get; private set; }
        public TimeSpan Duration { get; private set; }
    }
}