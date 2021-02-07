namespace Fixie.Internal.Listeners
{
    using System;
    using System.Collections.Generic;
    using Internal;
    using Reports;
    using static System.Environment;

    class ConsoleListener :
        Handler<CaseSkipped>,
        Handler<CasePassed>,
        Handler<CaseFailed>,
        Handler<AssemblyCompleted>
    {
        readonly bool outputCasePassed;
        bool paddingWouldRequireOpeningBlankLine;

        internal static ConsoleListener Create()
            => new ConsoleListener(GetEnvironmentVariable("FIXIE:TESTS") != null);

        public ConsoleListener(bool outputCasePassed = false)
            => this.outputCasePassed = outputCasePassed;

        public void Handle(CaseSkipped message)
        {
            var hasReason = message.Reason != null;

            WithPadding(() =>
            {
                using (Foreground.Yellow)
                    Console.WriteLine($"Test '{message.Name}' skipped{(hasReason ? ":" : null)}");

                if (hasReason)
                    Console.WriteLine($"{message.Reason}");
            });
        }

        public void Handle(CasePassed message)
        {
            if (outputCasePassed)
            {
                WithoutPadding(() =>
                {
                    using (Foreground.Green)
                        Console.WriteLine($"Test '{message.Name}' passed");
                });
            }
        }

        public void Handle(CaseFailed message)
        {
            WithPadding(() =>
            {
                using (Foreground.Red)
                    Console.WriteLine($"Test '{message.Name}' failed:");
                Console.WriteLine();
                Console.WriteLine(message.Exception.Message);
                Console.WriteLine();
                Console.WriteLine(message.Exception.GetType().FullName);
                Console.WriteLine(message.Exception.LiterateStackTrace());
            });
        }

        void WithPadding(Action write)
        {
            if (paddingWouldRequireOpeningBlankLine)
                Console.WriteLine();

            write();
            
            Console.WriteLine();
            paddingWouldRequireOpeningBlankLine = false;
        }

        void WithoutPadding(Action write)
        {
            write();
            paddingWouldRequireOpeningBlankLine = true;
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

            parts.Add($"took {message.Duration.TotalSeconds:0.00} seconds");

            return string.Join(", ", parts);
        }
    }
}