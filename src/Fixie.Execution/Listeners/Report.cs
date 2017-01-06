namespace Fixie.Execution.Listeners
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class Report
    {
        readonly List<ClassReport> classes;

        public Report(Assembly assembly)
        {
            Assembly = assembly;
            classes = new List<ClassReport>();
        }

        public void Add(ClassReport classReport) => classes.Add(classReport);

        public Assembly Assembly { get; }

        public TimeSpan Duration => new TimeSpan(classes.Sum(@class => @class.Duration.Ticks));

        public IReadOnlyList<ClassReport> Classes => classes;

        public int Passed => classes.Sum(@class => @class.Passed);
        public int Failed => classes.Sum(@class => @class.Failed);
        public int Skipped => classes.Sum(@class => @class.Skipped);
        public int Total => Passed + Failed + Skipped;
    }
}