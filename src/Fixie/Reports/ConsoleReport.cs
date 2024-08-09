using Fixie.Internal;
using static System.Environment;

namespace Fixie.Reports;

class ConsoleReport :
    IHandler<TestSkipped>,
    IHandler<TestPassed>,
    IHandler<TestFailed>,
    IHandler<ExecutionCompleted>
{
    readonly string? testPattern;
    readonly TextWriter console;
    readonly bool outputTestPassed;
    bool paddingWouldRequireOpeningBlankLine;

    internal static ConsoleReport Create(TestEnvironment environment)
        => new(environment, GetEnvironmentVariable("FIXIE_TESTS_PATTERN"));

    public ConsoleReport(TestEnvironment environment, string? testPattern = null)
    {
        console = environment.Console;
        this.testPattern = testPattern;
        this.outputTestPassed = testPattern != null;
        paddingWouldRequireOpeningBlankLine = true;
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
            {
                console.WriteLine(testPattern != null
                    ? $"No tests match the specified pattern: {testPattern}"
                    : "No tests found.");
            }
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
        List<string> parts = [];

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