using System;
using System.Collections.Generic;
using System.Reflection;
using Fixie.Results;

namespace Fixie.Execution
{
    public class FailResult
    {
        public FailResult(Case @case, AssertionLibraryFilter filter)
        {
            Name = @case.Name;
            Class = @case.Class;
            Method = @case.Method;
            Parameters = @case.Parameters;

            Output = @case.Output;
            Duration = @case.Duration;
            Exceptions = new CompoundException(@case.Execution.Exceptions, filter);
        }

        public string Name { get; private set; }
        public Type Class { get; private set; }
        public MethodInfo Method { get; private set; }
        public IReadOnlyList<object> Parameters { get; private set; }

        public string Output { get; private set; }
        public TimeSpan Duration { get; private set; }
        public CompoundException Exceptions { get; private set; }
    }
}