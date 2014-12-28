using System;
using System.Collections.Generic;
using System.Linq;

namespace Fixie.Execution
{
    [Serializable]
    public class ConventionResult
    {
        readonly List<ClassResult> classResults;

        public ConventionResult(string name)
        {
            classResults = new List<ClassResult>();
            Name = name;
        }

        public void Add(ClassResult classResult)
        {
            classResults.Add(classResult);
        }

        public string Name { get; private set; }

        public TimeSpan Duration
        {
            get { return new TimeSpan(classResults.Sum(result => result.Duration.Ticks)); }
        }

        public IReadOnlyList<ClassResult> ClassResults
        {
            get { return classResults; }
        }

        public int Passed { get { return classResults.Sum(result => result.Passed); } }

        public int Failed { get { return classResults.Sum(result => result.Failed); } }

        public int Skipped { get { return classResults.Sum(result => result.Skipped); } }

        public int Inconclusive { get { return classResults.Sum(result => result.Inconclusive); } }
    }
}