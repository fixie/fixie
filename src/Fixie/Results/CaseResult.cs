using System;

namespace Fixie.Results
{
    [Serializable]
    public class CaseResult
    {
        public CaseResult(string name, CaseStatus status, TimeSpan duration)
        {
            Name = name;
            Status = status;
            Duration = duration;
        }

        public string Name { get; private set; }
        public CaseStatus Status { get; private set; }
        public TimeSpan Duration { get; private set; }
    }
}