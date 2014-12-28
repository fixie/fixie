using System;
using System.Collections.Generic;
using System.Linq;

namespace Fixie.Execution
{
    [Serializable]
    public class ExecutionResult
    {
        readonly List<AssemblyResult> assemblyResults;

        public ExecutionResult()
        {
            assemblyResults = new List<AssemblyResult>();
        }

        public void Add(AssemblyResult assemblyResult)
        {
            assemblyResults.Add(assemblyResult);
        }

        public IReadOnlyList<AssemblyResult> AssemblyResults
        {
            get { return assemblyResults; }
        }

        public int Passed { get { return assemblyResults.Sum(result => result.Passed); } }

        public int Failed { get { return assemblyResults.Sum(result => result.Failed); } }

        public int Skipped { get { return assemblyResults.Sum(result => result.Skipped); } }

        public int Inconclusive { get { return assemblyResults.Sum(result => result.Inconclusive); } }

        public int Total
        {
            get { return Passed + Failed + Skipped + Inconclusive; }
        }
    }
}