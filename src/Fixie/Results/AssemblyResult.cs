using System;
using System.Collections.Generic;
using System.Linq;

namespace Fixie.Results
{
    [Serializable]
    public class AssemblyResult
    {
        readonly List<ConventionResult> conventionResults;

        public AssemblyResult(string name)
        {
            conventionResults = new List<ConventionResult>();
            Name = name;
        }

        public void Add(ConventionResult classResult)
        {
            conventionResults.Add(classResult);
        }

        public string Name { get; private set; }

        public TimeSpan Duration
        {
            get { return new TimeSpan(conventionResults.Sum(result => result.Duration.Ticks)); }
        }

        public IEnumerable<ConventionResult> ConventionResults
        {
            get { return conventionResults; }
        }

        public int Passed { get { return conventionResults.Sum(result => result.Passed); } }

        public int Failed { get { return conventionResults.Sum(result => result.Failed); } }

        public int Skipped { get { return conventionResults.Sum(result => result.Skipped); } }

        public int Total
        {
            get { return Passed + Failed + Skipped; }
        }
    }
}