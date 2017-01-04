namespace Fixie.Execution
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ClassReport
    {
        readonly List<CaseCompleted> cases;

        public ClassReport(string name)
        {
            cases = new List<CaseCompleted>();
            Name = name;
        }

        public void Add(CaseCompleted caseCompleted) => cases.Add(caseCompleted);

        public string Name { get; }

        public TimeSpan Duration => new TimeSpan(cases.Sum(result => result.Duration.Ticks));

        public IReadOnlyList<CaseCompleted> Cases => cases;

        public int Passed => cases.Count(@case => @case.Status == CaseStatus.Passed);
        public int Failed => cases.Count(@case => @case.Status == CaseStatus.Failed);
        public int Skipped => cases.Count(@case => @case.Status == CaseStatus.Skipped);
    }
}