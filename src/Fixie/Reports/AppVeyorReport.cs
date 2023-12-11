using System.Net.Http.Headers;
using System.Text;
using static System.Environment;
using static System.Text.Json.JsonSerializer;

namespace Fixie.Reports;

class AppVeyorReport(TestEnvironment environment, string uri, AppVeyorReport.PostAction action) :
        IHandler<ExecutionStarted>,
        IHandler<TestSkipped>,
        IHandler<TestPassed>,
        IHandler<TestFailed>
{
    public delegate Task PostAction(string uri, Result result);

    readonly string uri = new Uri(new Uri(uri), "api/tests").ToString();
    string runName = "Unknown";

    static readonly HttpClient Client;

    internal static AppVeyorReport? Create(TestEnvironment environment)
    {
        if (GetEnvironmentVariable("APPVEYOR") == "True")
        {
            var uri = GetEnvironmentVariable("APPVEYOR_API_URL");
            if (uri != null)
                return new AppVeyorReport(environment, uri, Post);
        }

        return null;
    }

    static AppVeyorReport()
    {
        Client = new HttpClient();
        Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public Task Handle(ExecutionStarted message)
    {
        runName = Path.GetFileNameWithoutExtension(environment.Assembly.Location);

        var framework = environment.TargetFramework;

        if (!string.IsNullOrEmpty(framework))
            runName = $"{runName} ({framework})";

        return Task.CompletedTask;
    }

    public async Task Handle(TestSkipped message)
    {
        await Post(new Result(runName, message, "Skipped")
        {
            ErrorMessage = message.Reason
        });
    }

    public async Task Handle(TestPassed message)
    {
        await Post(new Result(runName, message, "Passed"));
    }

    public async Task Handle(TestFailed message)
    {
        await Post(new Result(runName, message, "Failed")
        {
            ErrorMessage = message.Reason.Message,
            ErrorStackTrace =
                message.Reason.GetType().FullName +
                NewLine +
                message.Reason.StackTraceSummary()
        });
    }

    async Task Post(Result result)
    {
        await action(uri, result);
    }

    static async Task Post(string uri, Result result)
    {
        var content = Serialize(result);
        var response = await Client.PostAsync(uri, new StringContent(content, Encoding.UTF8, "application/json"));
        response.EnsureSuccessStatusCode();
    }

    public class Result(string runName, TestCompleted message, string outcome)
    {
        public string TestFramework { get; set; } = "Fixie";
        public string FileName { get; set; } = runName;
        public string TestName { get; set; } = message.TestCase;
        public string Outcome { get; set; } = outcome;
        public string DurationMilliseconds { get; set; } = $"{message.Duration.TotalMilliseconds:0}";
        public string StdOut { get; set; } = message.Output;
        public string? ErrorMessage { get; set; }
        public string? ErrorStackTrace { get; set; }
    }
}