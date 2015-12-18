using System;
using System.Collections.Generic;
using System.Linq;

namespace Fixie.Execution
{
    [Serializable]
    public class AssemblyReport
    {
        readonly List<ClassReport> classes;

        public AssemblyReport(string name)
        {
            classes = new List<ClassReport>();
            Name = name;
        }

        public void Add(ClassReport classReport) => classes.Add(classReport);

        public string Name { get; }

        public TimeSpan Duration => new TimeSpan(classes.Sum(classReport => classReport.Duration.Ticks));

        public IReadOnlyList<ClassReport> Classes => classes;

        public int Passed => classes.Sum(classReport => classReport.Passed);
        public int Failed => classes.Sum(classReport => classReport.Failed);
        public int Skipped => classes.Sum(classReport => classReport.Skipped);

        public int Total => Passed + Failed + Skipped;
    }
}