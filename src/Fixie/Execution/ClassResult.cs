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

        public void Add(CaseResult caseResult)
        {
            caseResults.Add(caseResult);
        }

        public string Name { get; private set; }

        public TimeSpan Duration
        {
            get { return new TimeSpan(caseResults.Sum(result => result.Duration.Ticks)); }
        }

        public IReadOnlyList<CaseResult> CaseResults
        {
            get { return caseResults; }
        }

        public int Passed { get { return caseResults.Count(result => result.Status == CaseStatus.Passed); } }

        public int Failed { get { return caseResults.Count(result => result.Status == CaseStatus.Failed); } }

        public int Skipped { get { return caseResults.Count(result => result.Status == CaseStatus.Skipped); } }

        public int Inconclusive { get { return caseResults.Count(result => result.Status == CaseStatus.Inconclusive); } }
    }
}