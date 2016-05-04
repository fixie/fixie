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
            Console.WriteLine("------ Testing Assembly {0} ------", Path.GetFileName(message.Location));
            Console.WriteLine();
        }

        public void Handle(CaseSkipped message)
        {
            summary.Add(message);

            using (Foreground.Yellow)
                Console.WriteLine("Test '{0}' skipped{1}", message.Name, message.SkipReason == null ? null : ": " + message.SkipReason);
        }

        public void Handle(CasePassed message)
        {
            summary.Add(message);
        }

        public void Handle(CaseFailed message)
        {
            summary.Add(message);

            using (Foreground.Red)
                Console.WriteLine("Test '{0}' failed: {1}", message.Name, message.Exceptions.PrimaryException.DisplayName);
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