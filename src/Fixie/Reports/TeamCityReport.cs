namespace Fixie.Reports;

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Environment;

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
        Message("testSuiteStarted name='{0}'", environment.Assembly.GetName().Name);
        return Task.CompletedTask;
    }

    public Task Handle(TestSkipped message)
    {
        TestStarted(message);
        Output(message);
        Message("testIgnored name='{0}' message='{1}'", message.TestCase, message.Reason);
        TestFinished(message);
        return Task.CompletedTask;
    }

    public Task Handle(TestPassed message)
    {
        TestStarted(message);
        Output(message);
        TestFinished(message);
        return Task.CompletedTask;
    }

    public Task Handle(TestFailed message)
    {
        var details =
            message.Reason.GetType().FullName +
            NewLine +
            message.Reason.LiterateStackTrace();

        TestStarted(message);
        Output(message);
        Message("testFailed name='{0}' message='{1}' details='{2}'", message.TestCase, message.Reason.Message, details);
        TestFinished(message);
        return Task.CompletedTask;
    }

    public Task Handle(ExecutionCompleted message)
    {
        Message("testSuiteFinished name='{0}'", environment.Assembly.GetName().Name);
        return Task.CompletedTask;
    }

    void TestStarted(TestCompleted message)
    {
        Message("testStarted name='{0}'", message.TestCase);
    }

    void TestFinished(TestCompleted message)
    {
        Message("testFinished name='{0}' duration='{1}'", message.TestCase, $"{message.Duration.TotalMilliseconds:0}");
    }

    void Message(string format, params string?[] args)
    {
        var encodedArgs = args.Select(Encode).Cast<object>().ToArray();
        environment.Console.WriteLine("##teamcity[" + format + "]", encodedArgs);
    }

    void Output(TestCompleted message)
    {
        if (!string.IsNullOrEmpty(message.Output))
            Message("testStdOut name='{0}' out='{1}'", message.TestCase, message.Output);
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