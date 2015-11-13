using System;
using System.Collections.Generic;
using System.Linq;

namespace Fixie.Execution
{
    [Serializable]
    public class ClassResult
    {
        readonly List<CaseResult> caseResults;

        public ClassResult(string name)
        {
            caseResults = new List<CaseResult>();
            Name = name;
        }

        public void Add(CaseResult caseResult) => caseResults.Add(caseResult);

        public string Name { get; }

        public TimeSpan Duration => new TimeSpan(caseResults.Sum(result => result.Duration.Ticks));

        public IReadOnlyList<CaseResult> CaseResults => caseResults;

        public int Passed => caseResults.Count(result => result.Status == CaseStatus.Passed);
        public int Failed => caseResults.Count(result => result.Status == CaseStatus.Failed);
        public int Skipped => caseResults.Count(result => result.Status == CaseStatus.Skipped);
    }
}