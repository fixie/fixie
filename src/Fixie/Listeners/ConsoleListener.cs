using System;
using System.IO;
using System.Text;
using Fixie.Execution;
using Fixie.Results;

namespace Fixie.Listeners
{
    public class ConsoleListener : Listener
    {
        public void AssemblyStarted(AssemblyInfo assembly)
        {
            Console.WriteLine("------ Testing Assembly {0} ------", Path.GetFileName(assembly.Location));
            Console.WriteLine();
        }

        public void CaseSkipped(SkipResult result)
        {
            using (Foreground.Yellow)
                Console.WriteLine("Test '{0}' skipped{1}", result.Name, result.Reason == null ? null : ": " + result.Reason);
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

        public void AssemblyCompleted(AssemblyInfo assembly, AssemblyResult result)
        {
            Console.WriteLine(result.Summary);
            Console.WriteLine();
        }
    }
}