using System;
using System.Collections.Generic;

namespace Fixie.Results
{
    [Serializable]
    public class CaseResult
    {
        public static CaseResult Passed(string name, IReadOnlyList<Trait> traits, TimeSpan duration)
        {
            return new CaseResult(name, traits, CaseStatus.Passed)
            {
                Duration = duration
            };
        }

        public static CaseResult Failed(string name, IReadOnlyList<Trait> traits, TimeSpan duration, CompoundException exceptions)
        {
            return new CaseResult(name, traits, CaseStatus.Failed)
            {
                Duration = duration,
                Exceptions = exceptions
            };
        }

        public static CaseResult Skipped(string name, IReadOnlyList<Trait> traits, string skipReason)
        {
            return new CaseResult(name, traits, CaseStatus.Skipped)
                   {
                       SkipReason = skipReason
                   };
        }

        CaseResult(string name, IReadOnlyList<Trait> traits, CaseStatus status)
        {
            Name = name;
            Status = status;
            Traits = traits;
        }

        public string Name { get; private set; }
        public CaseStatus Status { get; private set; }
        public IReadOnlyList<Trait> Traits { get; private set; }
        public TimeSpan Duration { get; private set; }

        public CompoundException Exceptions { get; private set; }

        public string SkipReason { get; private set; }
    }
}