using System;
using System.Collections.Generic;
using System.Linq;

namespace Fixie.Execution
{
    public class ClassReport
    {
        readonly List<CaseResult> cases;

        public ClassReport(string name)
        {
            cases = new List<CaseResult>();
            Name = name;
        }

        public void Add(CaseResult caseResult) => cases.Add(caseResult);

        public string Name { get; }

        public TimeSpan Duration => new TimeSpan(cases.Sum(@case => @case.Duration.Ticks));

        public IReadOnlyList<CaseResult> Cases => cases;

        public int Passed => cases.Count(@case => @case.Status == CaseStatus.Passed);
        public int Failed => cases.Count(@case => @case.Status == CaseStatus.Failed);
        public int Skipped => cases.Count(@case => @case.Status == CaseStatus.Skipped);
    }
}