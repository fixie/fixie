using System;
using System.IO;
using System.Text;
using Fixie.Execution;

namespace Fixie.ConsoleRunner
{
    public class ConsoleListener :
        IHandler<AssemblyStarted>,
        IHandler<CaseResult>,
        IHandler<AssemblyCompleted>
    {
        public void Handle(AssemblyStarted message)
        {
            Console.WriteLine($"------ Testing Assembly {Path.GetFileName(message.Location)} ------");
            Console.WriteLine();
        }

        public void Handle(CaseResult message)
        {
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

            var summary = new StringBuilder();

            summary.Append($"{message.Passed} passed");
            summary.Append($", {message.Failed} failed");

            if (message.Skipped > 0)
                summary.Append($", {message.Skipped} skipped");

            summary.Append($", took {message.Duration.TotalSeconds:N2} seconds");

            summary.Append($" ({name} {version}).");

            Console.WriteLine(summary.ToString());
            Console.WriteLine();
        }
    }
}