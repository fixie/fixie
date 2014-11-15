using System;
using System.Collections.Generic;

namespace Fixie.Execution
{
    [Serializable]
    public class PassResult
    {
        public PassResult(Case @case)
        {
            Name = @case.Name;
            Parameters = @case.Parameters;

            Output = @case.Output;
            Duration = @case.Duration;
            ReturnValue = @case.ReturnValue;
        }

        public string Name { get; private set; }
        public IReadOnlyList<object> Parameters { get; private set; }
        
        public string Output { get; private set; }
        public TimeSpan Duration { get; private set; }
        public object ReturnValue { get; set; }
    }
}