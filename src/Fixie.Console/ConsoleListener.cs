using System;
using System.IO;
using Fixie.Execution;

namespace Fixie.ConsoleRunner
{
    public class ConsoleListener :
        IHandler<AssemblyStarted>,
        IHandler<CaseCompleted>,
        IHandler<AssemblyCompleted>
    {
        ExecutionSummary summary;

        public void Handle(AssemblyStarted message)
        {
            summary = new ExecutionSummary();
            Console.WriteLine($"------ Testing Assembly {Path.GetFileName(message.Location)} ------");
            Console.WriteLine();
        }

        public void Handle(CaseCompleted message)
        {
            summary.Include(message);

            if (message.Status == CaseStatus.Failed)
            {
                using (Foreground.Red)
                    Console.WriteLine($"Test '{message.Name}' failed: {(message.AssertionFailed ? null : message.ExceptionType)}");
                Console.WriteLine(message.Message);
                Console.WriteLine(message.StackTrace);
                Console.WriteLine();
            }
            else if (message.Status == CaseStatus.Skipped)
            {
                var optionalReason = message.Message == null ? null : ": " + message.Message;

                using (Foreground.Yellow)
                    Console.WriteLine($"Test '{message.Name}' skipped{optionalReason}");
            }
        }

        public void Handle(AssemblyCompleted message)
        {
            var assemblyName = typeof(Convention).Assembly.GetName();
            var name = assemblyName.Name;
            var version = assemblyName.Version;

            Console.WriteLine($"{summary} ({name} {version}).");
            Console.WriteLine();
        }
    }
}