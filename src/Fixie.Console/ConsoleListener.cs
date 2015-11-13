using System;
using System.IO;
using System.Reflection;
using Fixie.Execution;

namespace Fixie.ConsoleRunner
{
    public class ConsoleListener : Listener
    {
        public void AssemblyStarted(Assembly assembly)
        {
            Console.WriteLine($"------ Testing Assembly {Path.GetFileName(assembly.Location)} ------");
            Console.WriteLine();
        }

        public void CaseSkipped(SkipResult result)
        {
            var optionalReason = result.SkipReason == null ? null : ": " + result.SkipReason;

            using (Foreground.Yellow)
                Console.WriteLine($"Test '{result.Name}' skipped{optionalReason}");
        }

        public void CasePassed(PassResult result)
        {
        }

        public void CaseFailed(FailResult result)
        {
            using (Foreground.Red)
                Console.WriteLine($"Test '{result.Name}' failed: {result.Exceptions.PrimaryException.DisplayName}");
            Console.WriteLine(result.Exceptions.CompoundStackTrace);
            Console.WriteLine();
        }

        public void AssemblyCompleted(Assembly assembly, AssemblyResult result)
        {
            Console.WriteLine(result.Summary);
            Console.WriteLine();
        }
    }
}