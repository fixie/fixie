namespace Fixie.Reports
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using static System.Environment;
    using static System.Text.Json.JsonSerializer;

    class AppVeyorReport :
        IHandler<ExecutionStarted>,
        IHandler<TestSkipped>,
        IHandler<TestPassed>,
        IHandler<TestFailed>
    {
        public delegate Task PostAction(string uri, Result result);

        readonly TestEnvironment environment;
        readonly PostAction postAction;
        readonly string uri;
        string runName;

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

        public AppVeyorReport(TestEnvironment environment, string uri, PostAction postAction)
        {
            this.environment = environment;
            this.postAction = postAction;
            this.uri = new Uri(new Uri(uri), "api/tests").ToString();
            runName = "Unknown";
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
            await postAction(uri, result);
        }

        static async Task Post(string uri, Result result)
        {
            var content = Serialize(result);
            var response = await Client.PostAsync(uri, new StringContent(content, Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
        }

        public class Result
        {
            public Result(string runName, TestCompleted message, string outcome)
            {
                TestFramework = "Fixie";
                FileName = runName;
                TestName = message.TestCase;
                Outcome = outcome;
                DurationMilliseconds = $"{message.Duration.TotalMilliseconds:0}";
                StdOut = message.Output;
            }

            public string TestFramework { get; set; }
            public string FileName { get; set; }
            public string TestName { get; set; }
            public string Outcome { get; set; }
            public string DurationMilliseconds { get; set; }
            public string StdOut { get; set; }
            public string? ErrorMessage { get; set; }
            public string? ErrorStackTrace { get; set; }
        }
    }
}