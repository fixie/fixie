namespace Fixie.Execution
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    [Serializable]
    public class AssemblyReport
    {
        readonly List<ConventionReport> conventions;

        public AssemblyReport(string location)
        {
            conventions = new List<ConventionReport>();
            Location = location;
        }

        public void Add(ConventionReport conventionReport) => conventions.Add(conventionReport);

        public string Location { get; }

        public TimeSpan Duration => new TimeSpan(conventions.Sum(result => result.Duration.Ticks));

        public IReadOnlyList<ConventionReport> Conventions => conventions;

        public int Passed => conventions.Sum(@class => @class.Passed);
        public int Failed => conventions.Sum(@class => @class.Failed);
        public int Skipped => conventions.Sum(@class => @class.Skipped);
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