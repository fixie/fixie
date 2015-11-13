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

        public void Add(AssemblyResult assemblyResult) => assemblyResults.Add(assemblyResult);

        public IReadOnlyList<AssemblyResult> AssemblyResults => assemblyResults;

        public int Passed => assemblyResults.Sum(result => result.Passed);
        public int Failed => assemblyResults.Sum(result => result.Failed);
        public int Skipped => assemblyResults.Sum(result => result.Skipped);

        public int Total => Passed + Failed + Skipped;
    }
}