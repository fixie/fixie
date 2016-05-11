using System;
using System.IO;
using Fixie.Execution;

namespace Fixie.ConsoleRunner
{
    public class ConsoleListener :
        Handler<AssemblyStarted>,
        Handler<CaseSkipped>,
        Handler<CasePassed>,
        Handler<CaseFailed>,
        Handler<AssemblyCompleted>
    {
        ExecutionSummary summary;

        public void Handle(AssemblyStarted message)
        {
            summary = new ExecutionSummary();
            Console.WriteLine($"------ Testing Assembly {Path.GetFileName(message.Location)} ------");
            Console.WriteLine();
        }

        public void Handle(CaseSkipped message)
        {
            summary.Add(message);

            var optionalReason = message.SkipReason == null ? null : ": " + message.SkipReason;

            using (Foreground.Yellow)
                Console.WriteLine($"Test '{message.Name}' skipped{optionalReason}");
        }

        public void Handle(CasePassed message)
        {
            summary.Add(message);
        }

        public void Handle(CaseFailed message)
        {
            summary.Add(message);

            using (Foreground.Red)
                Console.WriteLine($"Test '{message.Name}' failed: {message.Exceptions.PrimaryException.DisplayName}");
            Console.WriteLine(message.Exceptions.Message);
            Console.WriteLine(message.Exceptions.CompoundStackTrace);
            Console.WriteLine();
        }

        public void Handle(AssemblyCompleted message)
        {
            Console.WriteLine(summary);
            Console.WriteLine();
        }
    }
}