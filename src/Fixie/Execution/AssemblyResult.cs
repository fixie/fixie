using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fixie.Execution
{
    [Serializable]
    public class AssemblyResult
    {
        readonly List<ClassResult> classResults;

        public AssemblyResult(string name)
        {
            classResults = new List<ClassResult>();
            Name = name;
        }

        public void Add(ClassResult classResult) => classResults.Add(classResult);

        public string Name { get; }

        public TimeSpan Duration => new TimeSpan(classResults.Sum(result => result.Duration.Ticks));

        public IReadOnlyList<ClassResult> ClassResults => classResults;

        public int Passed => classResults.Sum(result => result.Passed);
        public int Failed => classResults.Sum(result => result.Failed);
        public int Skipped => classResults.Sum(result => result.Skipped);

        public int Total => Passed + Failed + Skipped;

        public string Summary
        {
            get
            {
                var assemblyName = typeof(AssemblyResult).Assembly.GetName();
                var name = assemblyName.Name;
                var version = assemblyName.Version;

                var line = new StringBuilder();

                line.Append($"{Passed} passed");
                line.Append($", {Failed} failed");

                if (Skipped > 0)
                    line.Append($", {Skipped} skipped");

                line.Append($", took {Duration.TotalSeconds:N2} seconds");

                line.Append($" ({name} {version}).");

                return line.ToString();
            }
        }
    }
}