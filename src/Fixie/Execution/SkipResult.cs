using System;
using System.Collections.Generic;

namespace Fixie.Execution
{
    [Serializable]
    public class SkipResult
    {
        public SkipResult(Case @case, string reason)
        {
            Name = @case.Name;
            Parameters = @case.Parameters;

            Reason = reason;
        }

        public string Name { get; private set; }
        public IReadOnlyList<object> Parameters { get; private set; }

        public string Reason { get; private set; }
    }
}