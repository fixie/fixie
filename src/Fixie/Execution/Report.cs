using System;
using System.Collections.Generic;
using System.Linq;

namespace Fixie.Execution
{
    [Serializable]
    public class Report
    {
        readonly List<AssemblyReport> assemblies;

        public Report()
        {
            assemblies = new List<AssemblyReport>();
        }

        public void Add(AssemblyReport assemblyReport) => assemblies.Add(assemblyReport);

        public IReadOnlyList<AssemblyReport> Assemblies => assemblies;

        public int Passed => assemblies.Sum(assemblyReport => assemblyReport.Passed);
        public int Failed => assemblies.Sum(assemblyReport => assemblyReport.Failed);
        public int Skipped => assemblies.Sum(assemblyReport => assemblyReport.Skipped);

        public int Total => Passed + Failed + Skipped;
    }
}