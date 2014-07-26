using System;

namespace Fixie.Execution
{
    public class PassResult
    {
        public PassResult(Case @case)
        {
            Case = @case;
            Output = @case.Execution.Output;
            Duration = @case.Execution.Duration;
        }

        public Case Case { get; private set; }
        public string Output { get; private set; }
        public TimeSpan Duration { get; private set; }
    }
}