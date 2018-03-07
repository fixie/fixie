namespace Fixie.Execution.Listeners
{
    using System;
    using System.Collections.Generic;
    using Cli;
    using Execution;

    public class ConsoleListener :
        Handler<CaseSkipped>,
        Handler<CaseFailed>,
        Handler<AssemblyCompleted>
    {
        public void Handle(CaseSkipped message)
        {
            var hasReason = message.Reason != null;

            using (Foreground.Yellow)
                Console.WriteLine($"Test '{message.Name}' skipped{(hasReason ? ":" : null)}");

            if (hasReason)
                Console.WriteLine($"{message.Reason}");

            Console.WriteLine();
        }

        public void Handle(CaseFailed message)
        {
            using (Foreground.Red)
                Console.WriteLine($"Test '{message.Name}' failed:");
            Console.WriteLine();
            Console.WriteLine(message.Exception.Message);
            Console.WriteLine();
            Console.WriteLine(message.Exception.TypeName());
            Console.WriteLine(message.Exception.LiterateStackTrace());
            Console.WriteLine();
        }

        public void Handle(AssemblyCompleted message)
        {
            Console.WriteLine(Summarize(message));
            Console.WriteLine();
        }

        static string Summarize(AssemblyCompleted message)
        {
            if (message.Total == 0)
                return "No tests found.";

            var parts = new List<string>();

            if (message.Passed > 0)
                parts.Add($"{message.Passed} passed");

            if (message.Failed > 0)
                parts.Add($"{message.Failed} failed");

            if (message.Skipped > 0)
                parts.Add($"{message.Skipped} skipped");

            parts.Add($"took {message.Duration.TotalSeconds:N2} seconds");

            return String.Join(", ", parts);
        }
    }
}