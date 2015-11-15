using System;
using Fixie.Internal;

namespace Fixie.Execution
{
    public class FailResult : IMessage
    {
        public FailResult(Case @case, AssertionLibraryFilter filter)
        {
            Name = @case.Name;
            MethodGroup = @case.MethodGroup;
            Output = @case.Output;
            Duration = @case.Duration;

            Exceptions = new CompoundException(@case.Exceptions, filter);
        }

        public string Name { get; }
        public MethodGroup MethodGroup { get; }
        public string Output { get; }
        public TimeSpan Duration { get; }
        public CompoundException Exceptions { get; }
    }
}