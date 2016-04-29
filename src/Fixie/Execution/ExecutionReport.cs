using System;
using System.Collections.Generic;
using System.Linq;

namespace Fixie.Execution
{
    [Serializable]
    public class ExecutionReport
    {
        readonly List<AssemblyReport> assemblies;

        public ExecutionReport()
        {
            assemblies = new List<AssemblyReport>();
        }

        public void Add(AssemblyReport assemblyReport)
        {
            assemblies.Add(assemblyReport);
        }

        public IReadOnlyList<AssemblyReport> Assemblies
        {
            get { return assemblies; }
        }

        public int Passed { get { return assemblies.Sum(result => result.Passed); } }

        public int Failed { get { return assemblies.Sum(result => result.Failed); } }

        public int Skipped { get { return assemblies.Sum(result => result.Skipped); } }

        public int Total
        {
            get { return Passed + Failed + Skipped; }
        }
    }
}