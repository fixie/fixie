using System;

namespace Fixie.Execution
{
    [Serializable]
    public class PassResult
    {
        public PassResult(Case @case)
        {
            Name = @case.Name;

            Output = @case.Output;
            Duration = @case.Duration;
        }

        public string Name { get; private set; }
        public string Output { get; private set; }
        public TimeSpan Duration { get; private set; }
    }
}