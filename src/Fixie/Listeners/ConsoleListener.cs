using System;
using System.Reflection;
using System.Text;
using Fixie.Results;

namespace Fixie.Listeners
{
    public class ConsoleListener : Listener
    {
        public void AssemblyStarted(Assembly assembly)
        {
            Console.WriteLine("------ Testing Assembly {0} ------", assembly.FileName());
            Console.WriteLine();
        }

        public void CaseSkipped(SkipResult result)
        {
            using (Foreground.Yellow)
                Console.WriteLine("Test '{0}' skipped{1}", result.Case.Name, result.Reason == null ? null : ": " + result.Reason);
        }

        public void CasePassed(PassResult result)
        {
        }

        public void CaseFailed(FailResult result)
        {
            var @case = result.Case;

            using (Foreground.Red)
                Console.WriteLine("Test '{0}' failed: {1}", @case.Name, result.PrimaryExceptionTypeName());
            result.WriteCompoundStackTraceTo(Console.Out);
            Console.WriteLine();
            Console.WriteLine();
        }

        public void AssemblyCompleted(Assembly assembly, AssemblyResult result)
        {
            var assemblyName = typeof(ConsoleListener).Assembly.GetName();
            var name = assemblyName.Name;
            var version = assemblyName.Version;

            var line = new StringBuilder();

            line.AppendFormat("{0} passed", result.Passed);
            line.AppendFormat(", {0} failed", result.Failed);

            if (result.Skipped > 0)
                line.AppendFormat(", {0} skipped", result.Skipped);

            line.AppendFormat(", took {0:N2} seconds", result.Duration.TotalSeconds);

            line.AppendFormat(" ({0} {1}).", name, version);
            Console.WriteLine(line);
            Console.WriteLine();
        }
    }
}