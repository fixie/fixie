using System;

namespace Fixie.Execution
{
    public class SkipResult
    {
        public SkipResult(Case @case, string skipReason)
        {
            Name = @case.Name;
            MethodGroup = @case.MethodGroup;
            SkipReason = skipReason;
        }

        public string Name { get; private set; }
        public MethodGroup MethodGroup { get; private set; }
        public string SkipReason { get; private set; }
    }
}