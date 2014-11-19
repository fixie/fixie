using System;
using Fixie.Results;

namespace Fixie.Execution
{
    [Serializable]
    public class FailResult
    {
        public FailResult(Case @case, AssertionLibraryFilter filter)
        {
            Name = @case.Name;

            Output = @case.Output;
            Duration = @case.Duration;
            Exceptions = new CompoundException(@case.Exceptions, filter);
        }

        public string Name { get; private set; }
        public string Output { get; private set; }
        public TimeSpan Duration { get; private set; }
        public CompoundException Exceptions { get; private set; }
    }
}