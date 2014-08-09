using System;
using System.Collections.Generic;
using System.Reflection;

namespace Fixie.Execution
{
    public class PassResult
    {
        public PassResult(Case @case)
        {
            Name = @case.Name;
            Class = @case.Class;
            Method = @case.Method;
            Parameters = @case.Parameters;

            Output = @case.Execution.Output;
            Duration = @case.Duration;
        }

        public string Name { get; private set; }
        public Type Class { get; private set; }
        public MethodInfo Method { get; private set; }
        public IReadOnlyList<object> Parameters { get; private set; }
        
        public string Output { get; private set; }
        public TimeSpan Duration { get; private set; }
    }
}