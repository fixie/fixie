namespace Fixie.Execution
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [Serializable]
    public class ClassResult
    {
        readonly List<CaseCompleted> caseResults;

        public ClassResult(string name)
        {
            caseResults = new List<CaseCompleted>();
            Name = name;
        }

        public void Add(CaseCompleted caseCompleted)
        {
            caseResults.Add(caseCompleted);
        }

        public string Name { get; private set; }

        public TimeSpan Duration
        {
            get { return new TimeSpan(caseResults.Sum(result => result.Duration.Ticks)); }
        }

        public IReadOnlyList<CaseCompleted> CaseResults
        {
            get { return caseResults; }
        }

        public int Passed { get { return caseResults.Count(result => result.Status == CaseStatus.Passed); } }

        public int Failed { get { return caseResults.Count(result => result.Status == CaseStatus.Failed); } }

        public int Skipped { get { return caseResults.Count(result => result.Status == CaseStatus.Skipped); } }
    }
}