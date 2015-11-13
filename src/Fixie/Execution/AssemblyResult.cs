using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fixie.Execution
{
    [Serializable]
    public class AssemblyResult
    {
        readonly List<ConventionResult> conventionResults;

        public AssemblyResult(string name)
        {
            conventionResults = new List<ConventionResult>();
            Name = name;
        }

        public void Add(ConventionResult classResult) => conventionResults.Add(classResult);

        public string Name { get; }

        public TimeSpan Duration => new TimeSpan(conventionResults.Sum(result => result.Duration.Ticks));

        public IReadOnlyList<ConventionResult> ConventionResults => conventionResults;

        public int Passed => conventionResults.Sum(result => result.Passed);
        public int Failed => conventionResults.Sum(result => result.Failed);
        public int Skipped => conventionResults.Sum(result => result.Skipped);

        public int Total => Passed + Failed + Skipped;

        public string Summary
        {
            get
            {
                var assemblyName = typeof(AssemblyResult).Assembly.GetName();
                var name = assemblyName.Name;
                var version = assemblyName.Version;

                var line = new StringBuilder();

                line.AppendFormat("{0} passed", Passed);
                line.AppendFormat(", {0} failed", Failed);

                if (Skipped > 0)
                    line.AppendFormat(", {0} skipped", Skipped);

                line.AppendFormat(", took {0:N2} seconds", Duration.TotalSeconds);

                line.AppendFormat(" ({0} {1}).", name, version);

                return line.ToString();
            }
        }
    }
}