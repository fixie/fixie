namespace Fixie.Reports
{
    using System;
    using System.Collections.Generic;
    using Internal;
    using static System.Environment;

    class ConsoleReport :
        Handler<TestSkipped>,
        Handler<TestPassed>,
        Handler<TestFailed>,
        Handler<AssemblyCompleted>
    {
        readonly bool outputTestPassed;
        bool paddingWouldRequireOpeningBlankLine;

        internal static ConsoleReport Create()
            => new ConsoleReport(GetEnvironmentVariable("FIXIE:TESTS") != null);

        public ConsoleReport(bool outputTestPassed = false)
            => this.outputTestPassed = outputTestPassed;

        public void Handle(TestSkipped message)
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

        public void Handle(TestPassed message)
        {
            if (outputTestPassed)
            {
                WithoutPadding(() =>
                {
                    using (Foreground.Green)
                        Console.WriteLine($"Test '{message.Name}' passed");
                });
            }
        }

        public void Handle(TestFailed message)
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