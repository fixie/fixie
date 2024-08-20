using System.Text;
using static System.Environment;

namespace Fixie.Reports;

class TeamCityReport :
    IHandler<ExecutionStarted>,
    IHandler<TestSkipped>,
    IHandler<TestPassed>,
    IHandler<TestFailed>,
    IHandler<ExecutionCompleted>
{
    readonly TestEnvironment environment;

    internal static TeamCityReport? Create(TestEnvironment environment)
    {
        if (GetEnvironmentVariable("TEAMCITY_PROJECT_NAME") != null)
            return new TeamCityReport(environment);

        return null;
    }

    public TeamCityReport(TestEnvironment environment)
    {
        this.environment = environment;
    }

    public Task Handle(ExecutionStarted message)
    {
        Message("##teamcity[testSuiteStarted name='{0}']", environment.Assembly.GetName().Name);
        return Task.CompletedTask;
    }

    public Task Handle(TestSkipped message)
    {
        Message("##teamcity[testStarted name='{0}']", message.TestCase);
        Message("##teamcity[testIgnored name='{0}' message='{1}']", message.TestCase, message.Reason);
        Message("##teamcity[testFinished name='{0}' duration='{1}']", message.TestCase, $"{message.Duration.TotalMilliseconds:0}");
        return Task.CompletedTask;
    }

    public Task Handle(TestPassed message)
    {
        Message("##teamcity[testStarted name='{0}']", message.TestCase);
        Message("##teamcity[testFinished name='{0}' duration='{1}']", message.TestCase, $"{message.Duration.TotalMilliseconds:0}");
        return Task.CompletedTask;
    }

    public Task Handle(TestFailed message)
    {
        var details =
            message.Reason.GetType().FullName +
            NewLine +
            message.Reason.StackTraceSummary();

        Message("##teamcity[testStarted name='{0}']", message.TestCase);
        Message("##teamcity[testFailed name='{0}' message='{1}' details='{2}']", message.TestCase, message.Reason.Message, details);
        Message("##teamcity[testFinished name='{0}' duration='{1}']", message.TestCase, $"{message.Duration.TotalMilliseconds:0}");
        return Task.CompletedTask;
    }

    public Task Handle(ExecutionCompleted message)
    {
        Message("##teamcity[testSuiteFinished name='{0}']", environment.Assembly.GetName().Name);
        return Task.CompletedTask;
    }

    void Message(string format, params string?[] args)
    {
        var encodedArgs = args.Select(Encode).Cast<object>().ToArray();
        environment.Console.WriteLine(format, encodedArgs);
    }

    static string Encode(string? value)
    {
        if (value == null)
            return "";

        var builder = new StringBuilder();

        foreach (var ch in value)
        {
            switch (ch)
            {
                case '|': builder.Append("||"); break;
                case '\'': builder.Append("|'"); break;
                case '[': builder.Append("|["); break;
                case ']': builder.Append("|]"); break;
                case '\n': builder.Append("|n"); break;
                case '\r': builder.Append("|r"); break;
                default:
                    if (RequiresHexEscape(ch))
                    {
                        builder.Append("|0x");
                        builder.Append(((int) ch).ToString("x4"));
                    }
                    else
                    {
                        builder.Append(ch);
                    }

                    break;
            }
        }

        return builder.ToString();
    }

    static bool RequiresHexEscape(char ch)
        => ch > '\x007f';
}