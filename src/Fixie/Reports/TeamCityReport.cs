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
        var assembly = Encode(environment.Assembly.GetName().Name);
        
        Message("##teamcity[testSuiteStarted name='{0}']", assembly);
        
        return Task.CompletedTask;
    }

    public Task Handle(TestSkipped message)
    {
        var testCase = Encode(message.TestCase);
        var reason = Encode(message.Reason);
        var duration = Encode($"{message.Duration.TotalMilliseconds:0}");
        
        Message("##teamcity[testStarted name='{0}']", testCase);
        Message("##teamcity[testIgnored name='{0}' message='{1}']", testCase, reason);
        Message("##teamcity[testFinished name='{0}' duration='{1}']", testCase, duration);
        
        return Task.CompletedTask;
    }

    public Task Handle(TestPassed message)
    {
        var testCase = Encode(message.TestCase);
        var duration = Encode($"{message.Duration.TotalMilliseconds:0}");
        
        Message("##teamcity[testStarted name='{0}']", testCase);
        Message("##teamcity[testFinished name='{0}' duration='{1}']", testCase, duration);
        
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
        var duration = Encode($"{message.Duration.TotalMilliseconds:0}");

        Message("##teamcity[testStarted name='{0}']", testCase);
        Message("##teamcity[testFailed name='{0}' message='{1}' details='{2}']", testCase, reason, details);
        Message("##teamcity[testFinished name='{0}' duration='{1}']", testCase, duration);
        
        return Task.CompletedTask;
    }

    public Task Handle(ExecutionCompleted message)
    {
        var assembly = Encode(environment.Assembly.GetName().Name);
        
        Message("##teamcity[testSuiteFinished name='{0}']", assembly);
        
        return Task.CompletedTask;
    }

    void Message(string format, params object?[] args)
    {
        environment.Console.WriteLine(format, args);
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