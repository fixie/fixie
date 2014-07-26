using System;
using Fixie.Results;

namespace Fixie.Execution
{
    public class FailResult
    {
        public FailResult(Case @case, AssertionLibraryFilter filter)
        {
            Case = @case;
            Output = @case.Execution.Output;
            Duration = @case.Execution.Duration;
            Exceptions = new CompoundException(@case.Execution.Exceptions, filter);
        }

        public Case Case { get; private set; }
        public string Output { get; private set; }
        public TimeSpan Duration { get; private set; }
        public CompoundException Exceptions { get; private set; }
    }
}