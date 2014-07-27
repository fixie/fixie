using System;
using System.Collections.Generic;
using System.Reflection;

namespace Fixie.Execution
{
    public class SkipResult
    {
        public SkipResult(Case @case, string reason)
        {
            Name = @case.Name;
            Class = @case.Class;
            Method = @case.Method;
            Parameters = @case.Parameters;

            Reason = reason;
        }

        public string Name { get; private set; }
        public Type Class { get; private set; }
        public MethodInfo Method { get; private set; }
        public IReadOnlyList<object> Parameters { get; private set; }

        public string Reason { get; private set; }
    }
}