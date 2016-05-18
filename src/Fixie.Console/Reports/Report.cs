namespace Fixie.ConsoleRunner.Reports
{
    using System.Collections.Generic;
    using System.Linq;

    public class Report
    {
        readonly List<AssemblyReport> assemblies = new List<AssemblyReport>();

        public void Add(AssemblyReport assemblyReport) => assemblies.Add(assemblyReport);

        public IReadOnlyList<AssemblyReport> Assemblies => assemblies;

        public int Passed => assemblies.Sum(result => result.Passed);
        public int Failed => assemblies.Sum(result => result.Failed);
        public int Skipped => assemblies.Sum(result => result.Skipped);
        public int Total => Passed + Failed + Skipped;
    }
}