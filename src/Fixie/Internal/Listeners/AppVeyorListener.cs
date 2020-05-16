namespace Fixie.Internal.Listeners
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Reflection;
    using System.Runtime.Versioning;
    using System.Text;
    using Internal;
    using static System.Environment;
    using static Serialization;

    class AppVeyorListener :
        Handler<AssemblyStarted>,
        Handler<CaseSkipped>,
        Handler<CasePassed>,
        Handler<CaseFailed>
    {
        public delegate void PostAction(string uri, TestResult testResult);

        readonly PostAction postAction;
        readonly string uri;
        string runName;

        static readonly HttpClient Client;

        internal static AppVeyorListener? Create()
        {
            if (GetEnvironmentVariable("APPVEYOR") == "True")
            {
                var uri = GetEnvironmentVariable("APPVEYOR_API_URL");
                if (uri != null)
                    return new AppVeyorListener(uri, Post);
            }

            return null;
        }

        static AppVeyorListener()
        {
            Client = new HttpClient();
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public AppVeyorListener(string uri, PostAction postAction)
        {
            this.postAction = postAction;
            this.uri = new Uri(new Uri(uri), "api/tests").ToString();
            runName = "Unknown";
        }

        public void Handle(AssemblyStarted message)
        {
            runName = Path.GetFileNameWithoutExtension(message.Assembly.Location);

            var framework = Assembly.GetEntryAssembly()?
                .GetCustomAttribute<TargetFrameworkAttribute>()?
                .FrameworkName;

            if (!string.IsNullOrEmpty(framework))
                runName = $"{runName} ({framework})";
        }

        public void Handle(CaseSkipped message)
        {
            Post(new TestResult(runName, message, "Skipped")
            {
                ErrorMessage = message.Reason
            });
        }

        public void Handle(CasePassed message)
        {
            Post(new TestResult(runName, message, "Passed"));
        }

        public void Handle(CaseFailed message)
        {
            Post(new TestResult(runName, message, "Failed")
            {
                ErrorMessage = message.Exception.Message,
                ErrorStackTrace =
                    message.Exception.GetType().FullName +
                    NewLine +
                    message.Exception.LiterateStackTrace()
            });
        }

        void Post(TestResult testResult)
        {
            postAction(uri, testResult);
        }

        static void Post(string uri, TestResult testResult)
        {
            var content = Serialize(testResult);
            Client.PostAsync(uri, new StringContent(content, Encoding.UTF8, "application/json"))
                .ContinueWith(x => x.Result.EnsureSuccessStatusCode())
                .Wait();
        }

        public class TestResult
        {
            public TestResult(string runName, CaseCompleted message, string outcome)
            {
                TestFramework = "Fixie";
                FileName = runName;
                TestName = message.Name;
                Outcome = outcome;
                DurationMilliseconds = message.Duration.TotalMilliseconds.ToString("0");
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