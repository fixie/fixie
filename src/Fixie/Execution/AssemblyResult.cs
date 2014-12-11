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

        public void Add(ConventionResult classResult)
        {
            conventionResults.Add(classResult);
        }

        public string Name { get; private set; }

        public TimeSpan Duration
        {
            get { return new TimeSpan(conventionResults.Sum(result => result.Duration.Ticks)); }
        }

        public IReadOnlyList<ConventionResult> ConventionResults
        {
            get { return conventionResults; }
        }

        public int Passed { get { return conventionResults.Sum(result => result.Passed); } }

        public int Failed { get { return conventionResults.Sum(result => result.Failed); } }

        public int Skipped { get { return conventionResults.Sum(result => result.Skipped); } }

        public int Total
        {
            get { return Passed + Failed + Skipped; }
        }

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