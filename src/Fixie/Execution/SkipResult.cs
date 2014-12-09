using System;
using Fixie.Discovery;

namespace Fixie.Execution
{
    [Serializable]
    public class SkipResult
    {
        public SkipResult(Case @case, string reason)
        {
            Name = @case.Name;
            MethodGroup = @case.MethodGroup;
            Reason = reason;
        }

        public string Name { get; private set; }
        public MethodGroup MethodGroup { get; private set; }
        public string Reason { get; private set; }
    }
}