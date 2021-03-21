namespace Fixie.Reports
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Reflection;
    using System.Runtime.Versioning;
    using System.Text;
    using System.Threading.Tasks;
    using static System.Environment;
    using static Internal.Serialization;

    class AppVeyorReport :
        Handler<AssemblyStarted>,
        AsyncHandler<TestSkipped>,
        AsyncHandler<TestPassed>,
        AsyncHandler<TestFailed>
    {
        public delegate Task PostAction(string uri, TestResult testResult);

        readonly PostAction postActionAsync;
        readonly string uri;
        string runName;

        static readonly HttpClient Client;

        internal static AppVeyorReport? Create()
        {
            if (GetEnvironmentVariable("APPVEYOR") == "True")
            {
                var uri = GetEnvironmentVariable("APPVEYOR_API_URL");
                if (uri != null)
                    return new AppVeyorReport(uri, PostAsync);
            }

            return null;
        }

        static AppVeyorReport()
        {
            Client = new HttpClient();
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public AppVeyorReport(string uri, PostAction postActionAsync)
        {
            this.postActionAsync = postActionAsync;
            this.uri = new Uri(new Uri(uri), "api/tests").ToString();
            runName = "Unknown";
        }

        public void Handle(AssemblyStarted message)
        {
            runName = Path.GetFileNameWithoutExtension(message.Assembly.Location);

            var framework = message.Assembly
                .GetCustomAttribute<TargetFrameworkAttribute>()?
                .FrameworkName;

            if (!string.IsNullOrEmpty(framework))
                runName = $"{runName} ({framework})";
        }

        public async Task HandleAsync(TestSkipped message)
        {
            await PostAsync(new TestResult(runName, message, "Skipped")
            {
                ErrorMessage = message.Reason
            });
        }

        public async Task HandleAsync(TestPassed message)
        {
            await PostAsync(new TestResult(runName, message, "Passed"));
        }

        public async Task HandleAsync(TestFailed message)
        {
            await PostAsync(new TestResult(runName, message, "Failed")
            {
                ErrorMessage = message.Reason.Message,
                ErrorStackTrace =
                    message.Reason.GetType().FullName +
                    NewLine +
                    message.Reason.LiterateStackTrace()
            });
        }

        async Task PostAsync(TestResult testResult)
        {
            await postActionAsync(uri, testResult);
        }

        static async Task PostAsync(string uri, TestResult testResult)
        {
            var content = Serialize(testResult);
            var response = await Client.PostAsync(uri, new StringContent(content, Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
        }

        public class TestResult
        {
            public TestResult(string runName, TestCompleted message, string outcome)
            {
                TestFramework = "Fixie";
                FileName = runName;
                TestName = message.Name;
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