namespace Fixie.Execution.Listeners
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Runtime.Serialization.Json;
    using System.Text;
    using Execution;

    public class AppVeyorListener :
        Handler<AssemblyStarted>,
        Handler<CaseSkipped>,
        Handler<CasePassed>,
        Handler<CaseFailed>
    {
        public delegate void PostAction(string uri, string mediaType, string content);

        readonly PostAction postAction;
        readonly string uri;
        string fileName;

        static readonly HttpClient Client;

        static AppVeyorListener()
        {
            Client = new HttpClient();
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public AppVeyorListener()
            : this(Environment.GetEnvironmentVariable("APPVEYOR_API_URL"), Post)
        {
        }

        public AppVeyorListener(string uri, PostAction postAction)
        {
            this.postAction = postAction;
            this.uri = new Uri(new Uri(uri), "api/tests").ToString();
        }

        public void Handle(AssemblyStarted message)
        {
            fileName = Path.GetFileName(message.Assembly.Location);
        }

        public void Handle(CaseSkipped message)
        {
            Post(message, x =>
            {
                x.Outcome = "Skipped";
                x.ErrorMessage = message.Reason;
            });
        }

        public void Handle(CasePassed message)
        {
            Post(message, x =>
            {
                x.Outcome = "Passed";
            });
        }

        public void Handle(CaseFailed message)
        {
            Post(message, x =>
            {
                x.Outcome = "Failed";
                x.ErrorMessage = message.Message;
                x.ErrorStackTrace = message.Exception.TypedStackTrace();
            });
        }

        void Post(CaseCompleted message, Action<TestResult> customize)
        {
            var testResult = new TestResult
            {
                TestFramework = "Fixie",
                FileName = fileName,
                TestName = message.Name,
                DurationMilliseconds = message.Duration.TotalMilliseconds.ToString("0"),
                StdOut = message.Output
            };

            customize(testResult);

            postAction(uri, "application/json", Serialize(testResult));
        }

        static string Serialize(TestResult testResult)
        {
            var serializer = new DataContractJsonSerializer(typeof(TestResult));

            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, testResult);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        static void Post(string uri, string mediaType, string content)
        {
            Client.PostAsync(uri, new StringContent(content, Encoding.UTF8, mediaType))
                .ContinueWith(x => x.Result.EnsureSuccessStatusCode())
                .Wait();
        }

        public class TestResult
        {
            public string TestFramework { get; set; }
            public string FileName { get; set; }
            public string TestName { get; set; }
            public string Outcome { get; set; }
            public string DurationMilliseconds { get; set; }
            public string StdOut { get; set; }
            public string ErrorMessage { get; set; }
            public string ErrorStackTrace { get; set; }
        }
    }
}