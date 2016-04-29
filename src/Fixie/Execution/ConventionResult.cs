using System;
using System.Collections.Generic;
using System.Linq;

namespace Fixie.Execution
{
    [Serializable]
    public class ConventionResult
    {
        readonly List<ClassReport> classes;

        public ConventionResult(string name)
        {
            classes = new List<ClassReport>();
            Name = name;
        }

        public void Add(ClassReport classReport)
        {
            classes.Add(classReport);
        }

        public string Name { get; private set; }

        public TimeSpan Duration
        {
            get { return new TimeSpan(classes.Sum(result => result.Duration.Ticks)); }
        }

        public IReadOnlyList<ClassReport> Classes
        {
            get { return classes; }
        }

        public int Passed { get { return classes.Sum(result => result.Passed); } }

        public int Failed { get { return classes.Sum(result => result.Failed); } }

        public int Skipped { get { return classes.Sum(result => result.Skipped); } }
    }
}