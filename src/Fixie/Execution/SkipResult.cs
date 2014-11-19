using System;

namespace Fixie.Execution
{
    [Serializable]
    public class SkipResult
    {
        public SkipResult(Case @case, string reason)
        {
            Name = @case.Name;
            Reason = reason;
        }

        public string Name { get; private set; }
        public string Reason { get; private set; }
    }
}