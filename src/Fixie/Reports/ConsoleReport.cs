namespace Fixie.Reports
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Internal;
    using static System.Environment;

    class ConsoleReport :
        Handler<TestSkipped>,
        Handler<TestPassed>,
        Handler<TestFailed>,
        Handler<AssemblyCompleted>
    {
        readonly TextWriter console;
        readonly bool outputTestPassed;
        bool paddingWouldRequireOpeningBlankLine;

        internal static ConsoleReport Create(TextWriter console)
            => new ConsoleReport(console, GetEnvironmentVariable("FIXIE:TESTS") != null);

        public ConsoleReport(TextWriter console, bool outputTestPassed = false)
        {
            this.console = console;
            this.outputTestPassed = outputTestPassed;
        }

        public void Handle(TestSkipped message)
        {
            var hasReason = message.Reason != null;

            WithPadding(() =>
            {
                using (Foreground.Yellow)
                    console.WriteLine($"Test '{message.Name}' skipped{(hasReason ? ":" : null)}");

                if (hasReason)
                    console.WriteLine($"{message.Reason}");
            });
        }

        public void Handle(TestPassed message)
        {
            if (outputTestPassed)
            {
                WithoutPadding(() =>
                {
                    using (Foreground.Green)
                        console.WriteLine($"Test '{message.Name}' passed");
                });
            }
        }

        public void Handle(TestFailed message)
        {
            WithPadding(() =>
            {
                using (Foreground.Red)
                    console.WriteLine($"Test '{message.Name}' failed:");
                console.WriteLine();
                console.WriteLine(message.Reason.Message);
                console.WriteLine();
                console.WriteLine(message.Reason.GetType().FullName);
                console.WriteLine(message.Reason.LiterateStackTrace());
            });
        }

        void WithPadding(Action write)
        {
            if (paddingWouldRequireOpeningBlankLine)
                console.WriteLine();

            write();
            
            console.WriteLine();
            paddingWouldRequireOpeningBlankLine = false;
        }

        void WithoutPadding(Action write)
        {
            write();
            paddingWouldRequireOpeningBlankLine = true;
        }

        public void Handle(AssemblyCompleted message)
        {
            console.WriteLine(Summarize(message));
            console.WriteLine();
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