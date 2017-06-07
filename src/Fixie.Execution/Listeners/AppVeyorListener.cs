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
                x.ErrorMessage = message.Reason;
            });
        }

        public void Handle(CasePassed message)
        {
            Post(message);
        }

        public void Handle(CaseFailed message)
        {
            var exception = message.Exception;

            Post(message, x =>
            {
                x.ErrorMessage = exception.Message;
                x.ErrorStackTrace = exception.TypedStackTrace();
            });
        }

        void Post(CaseCompleted message, Action<TestResult> customize = null)
        {
            var testResult = new TestResult
            {
                testFramework = "Fixie",
                fileName = fileName,
                testName = message.Name,
                outcome = message.Status.ToString(),
                durationMilliseconds = message.Duration.TotalMilliseconds.ToString("0"),
                StdOut = message.Output
            };

            customize?.Invoke(testResult);

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
            public string testFramework { get; set; }
            public string fileName { get; set; }
            public string testName { get; set; }
            public string outcome { get; set; }
            public string durationMilliseconds { get; set; }
            public string StdOut { get; set; }
            public string ErrorMessage { get; set; }
            public string ErrorStackTrace { get; set; }
        }
    }
}