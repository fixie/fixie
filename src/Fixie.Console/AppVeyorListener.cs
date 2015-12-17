using Fixie.Execution;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Script.Serialization;

namespace Fixie.ConsoleRunner
{
    public class AppVeyorListener :
        IHandler<AssemblyStarted>,
        IHandler<CaseResult>
    {
        readonly string url;
        readonly HttpClient client;
        string fileName;

        public AppVeyorListener()
            : this(Environment.GetEnvironmentVariable("APPVEYOR_API_URL"), new HttpClient())
        {
        }

        public AppVeyorListener(string url, HttpClient client)
        {
            this.url = new Uri(new Uri(url), "api/tests").ToString();
            this.client = client;
            this.client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public void Handle(AssemblyStarted message)
        {
            fileName = Path.GetFileName(message.Assembly.Location);
        }

        public void Handle(CaseResult message)
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

            if (message.Status == CaseStatus.Failed)
            {
                testResult.ErrorMessage = message.IsAssertionException ? "" : message.ExceptionType;
                testResult.ErrorStackTrace = message.StackTrace;
            }
            else if (message.Status == CaseStatus.Passed)
            {
                testResult.ErrorMessage = null;
                testResult.ErrorStackTrace = null;
            }
            else if (message.Status == CaseStatus.Skipped)
            {
                testResult.ErrorMessage = message.Message;
                testResult.ErrorStackTrace = null;
            }

            Post(testResult);
        }

        void Post(TestResult result)
        {
            var content = new JavaScriptSerializer().Serialize(result);
            client.PostAsync(url, new StringContent(content, Encoding.UTF8, "application/json"))
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