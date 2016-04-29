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

        public void Add(CaseCompleted message)
        {
            cases.Add(message);
        }

        public string Name { get; private set; }

        public TimeSpan Duration
        {
            get { return new TimeSpan(cases.Sum(result => result.Duration.Ticks)); }
        }

        public IReadOnlyList<CaseCompleted> Cases
        {
            get { return cases; }
        }

        public int Passed { get { return cases.Count(result => result.Status == CaseStatus.Passed); } }

        public int Failed { get { return cases.Count(result => result.Status == CaseStatus.Failed); } }

        public int Skipped { get { return cases.Count(result => result.Status == CaseStatus.Skipped); } }
    }
}