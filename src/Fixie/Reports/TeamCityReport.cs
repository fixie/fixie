using System.Text;
using static System.Environment;

namespace Fixie.Reports;

class TeamCityReport(TestEnvironment environment) :
    IHandler<ExecutionStarted>,
    IHandler<TestSkipped>,
    IHandler<TestPassed>,
    IHandler<TestFailed>,
    IHandler<ExecutionCompleted>
{
    internal static TeamCityReport? Create(TestEnvironment environment)
    {
        if (GetEnvironmentVariable("TEAMCITY_PROJECT_NAME") != null)
            return new TeamCityReport(environment);

        return null;
    }

    public Task Handle(ExecutionStarted message)
    {
        var assembly = Encode(environment.Assembly.GetName().Name);

        environment.Console.WriteLine($"##teamcity[testSuiteStarted name='{assembly}']");
        
        return Task.CompletedTask;
    }

    public Task Handle(TestSkipped message)
    {
        var testCase = Encode(message.TestCase);
        var reason = Encode(message.Reason);
        var duration = message.Duration.TotalMilliseconds;

        environment.Console.WriteLine($"##teamcity[testStarted name='{testCase}']");
        environment.Console.WriteLine($"##teamcity[testIgnored name='{testCase}' message='{reason}']");
        environment.Console.WriteLine($"##teamcity[testFinished name='{testCase}' duration='{duration:0}']");
        
        return Task.CompletedTask;
    }

    public Task Handle(TestPassed message)
    {
        var testCase = Encode(message.TestCase);
        var duration = message.Duration.TotalMilliseconds;

        environment.Console.WriteLine($"##teamcity[testStarted name='{testCase}']");
        environment.Console.WriteLine($"##teamcity[testFinished name='{testCase}' duration='{duration:0}']");
        
        return Task.CompletedTask;
    }

    public Task Handle(TestFailed message)
    {
        var testCase = Encode(message.TestCase);
        var reason = Encode(message.Reason.Message);
        var details =
            Encode(message.Reason.GetType().FullName +
                   NewLine +
                   message.Reason.StackTraceSummary());
        var duration = message.Duration.TotalMilliseconds;

        environment.Console.WriteLine($"##teamcity[testStarted name='{testCase}']");
        environment.Console.WriteLine($"##teamcity[testFailed name='{testCase}' message='{reason}' details='{details}']");
        environment.Console.WriteLine($"##teamcity[testFinished name='{testCase}' duration='{duration:0}']");
        
        return Task.CompletedTask;
    }

    public Task Handle(ExecutionCompleted message)
    {
        var assembly = Encode(environment.Assembly.GetName().Name);

        environment.Console.WriteLine($"##teamcity[testSuiteFinished name='{assembly}']");
        
        return Task.CompletedTask;
    }

    static string Encode(string? value)
    {
        if (value == null)
            return "";

        var builder = new StringBuilder();

        foreach (var ch in value)
        {
            var escapeSequence = ch switch
            {
                '|' => "||",
                '\'' => "|'",
                '[' => "|[",
                ']' => "|]",
                '\n' => "|n",
                '\r' => "|r",
                > '\x007f' => // Hex escape is required.
                    "|0x" + ((int)ch).ToString("x4"),
                _ => null
            };

            if (escapeSequence == null)
                builder.Append(ch);
            else
                builder.Append(escapeSequence);
        }

        return builder.ToString();
    }
}