using System;
using System.Collections.Generic;
using System.Linq;

namespace Fixie.ConsoleRunner
{
    public class AssemblyReport
    {
        readonly List<ClassReport> classes;

        public AssemblyReport(string location)
        {
            classes = new List<ClassReport>();
            Location = location;
        }

        public void Add(ClassReport classReport) => classes.Add(classReport);

        public string Location { get; }

        public TimeSpan Duration => new TimeSpan(classes.Sum(classReport => classReport.Duration.Ticks));

        public IReadOnlyList<ClassReport> Classes => classes;

        public int Passed => classes.Sum(classReport => classReport.Passed);
        public int Failed => classes.Sum(classReport => classReport.Failed);
        public int Skipped => classes.Sum(classReport => classReport.Skipped);

        public int Total => Passed + Failed + Skipped;
    }
}