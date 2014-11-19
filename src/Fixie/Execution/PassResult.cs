using System;
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

            Output = @case.Output;
            Duration = @case.Duration;
            ReturnValue = @case.ReturnValue;
        }

        public string Name { get; private set; }
        public Type Class { get; private set; }
        public MethodInfo Method { get; private set; }
        
        public string Output { get; private set; }
        public TimeSpan Duration { get; private set; }
        public object ReturnValue { get; set; }
    }
}