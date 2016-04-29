using System;
using System.Collections.Generic;
using System.Linq;

namespace Fixie.Execution
{
    [Serializable]
    public class ConventionReport
    {
        readonly List<ClassReport> classes;

        public ConventionReport(string name)
        {
            classes = new List<ClassReport>();
            Name = name;
        }

        public void Add(ClassReport classReport) => classes.Add(classReport);

        public string Name { get; }

        public TimeSpan Duration => new TimeSpan(classes.Sum(result => result.Duration.Ticks));

        public IReadOnlyList<ClassReport> Classes => classes;

        public int Passed => classes.Sum(result => result.Passed);
        public int Failed => classes.Sum(result => result.Failed);
        public int Skipped => classes.Sum(result => result.Skipped);
    }
}