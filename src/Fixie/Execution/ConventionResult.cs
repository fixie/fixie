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

        public void Add(ClassResult classResult) => classResults.Add(classResult);

        public string Name { get; }

        public TimeSpan Duration => new TimeSpan(classResults.Sum(result => result.Duration.Ticks));

        public IReadOnlyList<ClassResult> ClassResults => classResults;

        public int Passed => classResults.Sum(result => result.Passed);
        public int Failed => classResults.Sum(result => result.Failed);
        public int Skipped => classResults.Sum(result => result.Skipped);
    }
}