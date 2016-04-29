using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fixie.Execution
{
    [Serializable]
    public class AssemblyResult
    {
        readonly List<ConventionReport> conventions;

        public AssemblyResult(string name)
        {
            conventions = new List<ConventionReport>();
            Name = name;
        }

        public void Add(ConventionReport conventionReport)
        {
            conventions.Add(conventionReport);
        }

        public string Name { get; private set; }

        public TimeSpan Duration
        {
            get { return new TimeSpan(conventions.Sum(result => result.Duration.Ticks)); }
        }

        public IReadOnlyList<ConventionReport> Conventions
        {
            get { return conventions; }
        }

        public int Passed { get { return conventions.Sum(result => result.Passed); } }

        public int Failed { get { return conventions.Sum(result => result.Failed); } }

        public int Skipped { get { return conventions.Sum(result => result.Skipped); } }

        public int Total
        {
            get { return Passed + Failed + Skipped; }
        }

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