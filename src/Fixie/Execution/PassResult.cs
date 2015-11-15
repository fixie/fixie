using System;

namespace Fixie.Execution
{
    public class PassResult : IMessage
    {
        public PassResult(Case @case)
        {
            Name = @case.Name;
            MethodGroup = @case.MethodGroup;
            Output = @case.Output;
            Duration = @case.Duration;
        }

        public string Name { get; }
        public MethodGroup MethodGroup { get; }
        public string Output { get; }
        public TimeSpan Duration { get; }
    }
}