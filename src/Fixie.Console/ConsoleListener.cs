using System;
using System.IO;
using System.Reflection;
using Fixie.Execution;

namespace Fixie.ConsoleRunner
{
    public class ConsoleListener : Listener
    {
        public void AssemblyStarted(AssemblyStarted message)
        {
            Console.WriteLine($"------ Testing Assembly {Path.GetFileName(message.Assembly.Location)} ------");
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

        public void AssemblyCompleted(AssemblyCompleted message)
        {
            Console.WriteLine(message.Result.Summary);
            Console.WriteLine();
        }
    }
}