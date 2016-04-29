using System;
using System.Collections.Generic;
using System.Linq;

namespace Fixie.Execution
{
    [Serializable]
    public class ClassReport
    {
        readonly List<CaseCompleted> cases;

        public ClassReport(string name)
        {
            cases = new List<CaseCompleted>();
            Name = name;
        }

        public void Add(CaseCompleted message) => cases.Add(message);

        public string Name { get; }

        public TimeSpan Duration => new TimeSpan(cases.Sum(result => result.Duration.Ticks));

        public IReadOnlyList<CaseCompleted> Cases => cases;

        public int Passed => cases.Count(@case => @case.Status == CaseStatus.Passed);
        public int Failed => cases.Count(@case => @case.Status == CaseStatus.Failed);
        public int Skipped => cases.Count(@case => @case.Status == CaseStatus.Skipped);
    }
}