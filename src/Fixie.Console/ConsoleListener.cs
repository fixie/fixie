using System;
using System.IO;
using System.Text;
using Fixie.Execution;

namespace Fixie.ConsoleRunner
{
    public class ConsoleListener : MarshalByRefObject, Listener
    {
        public void AssemblyStarted(AssemblyInfo assembly)
        {
            Console.WriteLine("------ Testing Assembly {0} ------", Path.GetFileName(assembly.Location));
            Console.WriteLine();
        }

        public void CaseSkipped(SkipResult result)
        {
            using (Foreground.Yellow)
                Console.WriteLine("Test '{0}' skipped{1}", result.Name, result.SkipReason == null ? null : ": " + result.SkipReason);
        }

        public void CasePassed(PassResult result)
        {
        }

        public void CaseFailed(FailResult result)
        {
            using (Foreground.Red)
                Console.WriteLine("Test '{0}' failed: {1}", result.Name, result.Exceptions.PrimaryException.DisplayName);
            Console.WriteLine(result.Exceptions.CompoundStackTrace);
            Console.WriteLine();
        }

        public void CaseInconclusive(InconclusiveResult result)
        {
            if (result.Exceptions == null)
            {
                using (Foreground.Yellow)
                    Console.WriteLine("Test '{0}' was inconclusive", result.Name);
            }
            else
            {
                using (Foreground.Yellow)
                    Console.WriteLine("Test '{0}' was inconclusive: {1}", result.Name, result.Exceptions.PrimaryException.DisplayName);
                Console.WriteLine(result.Exceptions.CompoundStackTrace);
                Console.WriteLine();
            }
        }

        public void AssemblyCompleted(AssemblyInfo assembly, AssemblyResult result)
        {
            var assemblyName = typeof(Convention).Assembly.GetName();
            var name = assemblyName.Name;
            var version = assemblyName.Version;

            var line = new StringBuilder();

            line.AppendFormat("{0} passed", result.Passed);
            line.AppendFormat(", {0} failed", result.Failed);

            if (result.Skipped > 0)
                line.AppendFormat(", {0} skipped", result.Skipped);

            if (result.Inconclusive > 0)
                line.AppendFormat(", {0} inconclusive", result.Inconclusive);

            line.AppendFormat(", took {0:N2} seconds", result.Duration.TotalSeconds);

            line.AppendFormat(" ({0} {1}).", name, version);

            Console.WriteLine(line);
            Console.WriteLine();
        }
    }
}