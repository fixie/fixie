namespace Fixie.Reports
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using static System.Environment;
    using static Internal.Serialization;

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
                var appVeyorApiUrl = GetEnvironmentVariable("APPVEYOR_API_URL");
                if (appVeyorApiUrl != null)
                    return new AppVeyorReport(environment, appVeyorApiUrl, Post);
            }

            return null;
        }

#pragma warning disable S3963 // "static" fields should be initialized inline
        static AppVeyorReport()
        {
            Client = new HttpClient();
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
#pragma warning restore S3963 // "static" fields should be initialized inline

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

        public Task Handle(TestSkipped message)
        {
            return Post(new Result(runName, message, "Skipped")
            {
                ErrorMessage = message.Reason
            });
        }

        public Task Handle(TestPassed message)
        {
            return Post(new Result(runName, message, "Passed"));
        }

        public Task Handle(TestFailed message)
        {
            return Post(new Result(runName, message, "Failed")
            {
                ErrorMessage = message.Reason.Message,
                ErrorStackTrace =
                    message.Reason.GetType().FullName +
                    NewLine +
                    message.Reason.LiterateStackTrace()
            });
        }

        Task Post(Result result)
        {
            return postAction(uri, result);
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