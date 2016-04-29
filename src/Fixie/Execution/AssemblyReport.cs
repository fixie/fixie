using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public TimeSpan Duration => new TimeSpan(classes.Sum(result => result.Duration.Ticks));

        public IReadOnlyList<ClassReport> Classes => classes;

        public int Passed => classes.Sum(result => result.Passed);
        public int Failed => classes.Sum(result => result.Failed);
        public int Skipped => classes.Sum(result => result.Skipped);
        public int Total => Passed + Failed + Skipped;

        public string Summary
        {
            get
            {
                var line = new StringBuilder();

                line.AppendFormat("{0} passed", Passed);
                line.AppendFormat(", {0} failed", Failed);

                if (Skipped > 0)
                    line.AppendFormat(", {0} skipped", Skipped);

                line.AppendFormat(", took {0:N2} seconds", Duration.TotalSeconds);

                line.AppendFormat(" ({0}).", Framework.Version);

                return line.ToString();
            }
        }
    }
}