using System;
using System.Reflection;

namespace Fixie.Execution
{
    [Serializable]
    public class AssemblyCompleted : IMessage
    {
        public AssemblyCompleted(Assembly assembly, AssemblyReport assemblyReport)
        {
            var assemblyName = assembly.GetName();

            Name = assemblyName.Name;
            FullName = assemblyName.FullName;
            Version = assemblyName.Version.ToString();
            Location = assembly.Location;

            Passed = assemblyReport.Passed;
            Failed = assemblyReport.Failed;
            Skipped = assemblyReport.Skipped;
            Duration = assemblyReport.Duration;
        }

        public string Name { get; }
        public string FullName { get; }
        public string Version { get; }
        public string Location { get; }

        public int Passed { get; }
        public int Failed { get; }
        public int Skipped { get; }
        public TimeSpan Duration { get; }
    }
}