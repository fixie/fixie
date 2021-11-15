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
            => new ConsoleReport(environment, GetEnvironmentVariable("FIXIE:TESTS_PATTERN") != null);

        public ConsoleReport(TestEnvironment environment, bool outputTestPassed = false)
        {
            console = environment.Console;
            this.outputTestPassed = outputTestPassed;
        }

        public Task Handle(TestSkipped message)
        {
            return WithPadding(async () =>
            {
                using (Foreground.Yellow)
                    await console.WriteLineAsync($"Test '{message.TestCase}' skipped:");

                await console.WriteLineAsync(message.Reason);
            });
        }

        public async Task Handle(TestPassed message)
        {
            if (outputTestPassed)
            {
                await WithoutPadding(async () =>
                {
                    using (Foreground.Green)
                        await console.WriteLineAsync($"Test '{message.TestCase}' passed");
                });
            }
        }

        public async Task Handle(TestFailed message)
        {
            await WithPadding(async () =>
            {
                using (Foreground.Red)
                    await console.WriteLineAsync($"Test '{message.TestCase}' failed:");
                await console.WriteLineAsync();
                await console.WriteLineAsync(message.Reason.Message);
                await console.WriteLineAsync();
                await console.WriteLineAsync(message.Reason.GetType().FullName);
                await console.WriteLineAsync(message.Reason.LiterateStackTrace());
            });
        }

        async Task WithPadding(Func<Task> write)
        {
            if (paddingWouldRequireOpeningBlankLine)
                await console.WriteLineAsync();

            await write();
            
            await console.WriteLineAsync();
            paddingWouldRequireOpeningBlankLine = false;
        }

        async Task WithoutPadding(Func<Task> write)
        {
            await write();
            paddingWouldRequireOpeningBlankLine = true;
        }

        public async Task Handle(ExecutionCompleted message)
        {
            await console.WriteLineAsync(Summarize(message));
            await console.WriteLineAsync();
        }

        static string Summarize(ExecutionCompleted message)
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