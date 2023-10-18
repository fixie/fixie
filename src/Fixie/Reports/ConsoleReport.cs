namespace Fixie.Reports
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Internal;
    using static System.Environment;

    class ConsoleReport :
        IHandler<TestSkipped>,
        IHandler<TestPassed>,
        IHandler<TestFailed>,
        IHandler<ExecutionCompleted>
    {
        readonly TextWriter console;
        readonly bool outputTestPassed;
        bool paddingWouldRequireOpeningBlankLine;

        internal static ConsoleReport Create(TestEnvironment environment)
            => new ConsoleReport(environment, GetEnvironmentVariable("FIXIE_TESTS_PATTERN") != null);

        public ConsoleReport(TestEnvironment environment, bool outputTestPassed = false)
        {
            console = environment.Console;
            this.outputTestPassed = outputTestPassed;
        }

        public Task Handle(TestSkipped message)
        {
            WithPadding(() =>
            {
                using (Foreground.Yellow)
                    console.WriteLine($"Test '{message.TestCase}' skipped:");

                console.WriteLine(message.Reason);
            });

            return Task.CompletedTask;
        }

        public Task Handle(TestPassed message)
        {
            if (outputTestPassed)
            {
                WithoutPadding(() =>
                {
                    using (Foreground.Green)
                        console.WriteLine($"Test '{message.TestCase}' passed");
                });
            }

            return Task.CompletedTask;
        }

        public Task Handle(TestFailed message)
        {
            WithPadding(() =>
            {
                using (Foreground.Red)
                    console.WriteLine($"Test '{message.TestCase}' failed:");
                console.WriteLine();
                console.WriteLine(message.Reason.Message);
                console.WriteLine();
                console.WriteLine(message.Reason.GetType().FullName);
                console.WriteLine(message.Reason.StackTraceSummary());
            });

            return Task.CompletedTask;
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

        public Task Handle(ExecutionCompleted message)
        {
            if (message.Total == 0)
            {
                using (Foreground.Red)
                    console.WriteLine("No tests found.");
            }
            else
            {
                console.WriteLine(Summarize(message));
            }

            console.WriteLine();

            return Task.CompletedTask;
        }

        static string Summarize(ExecutionCompleted message)
        {
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