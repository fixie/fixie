using Fixie.Execution;
using Fixie.Results;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Web.Script.Serialization;

namespace Fixie.Listeners
{
    public class AppVeyorListener : Listener
    {
        readonly string url;
        readonly HttpClient client;

        public AppVeyorListener(string url)
            : this(url, new HttpClient())
        {
        }

        public AppVeyorListener(string url, HttpClient client)
        {
            this.url = new Uri(new Uri(url), "api/tests").ToString();
            this.client = client;
            this.client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public void AssemblyStarted(Assembly assembly)
        {
        }

        public void CaseSkipped(SkipResult result)
        {
            Post(new TestResult
            {
                fileName = Path.GetFileName(result.Class.Assembly.Location),
                outcome = "Skipped",
                testFramework = "fixie",
                testName = result.Name
            });
        }

        public void CasePassed(PassResult result)
        {
            Post(new TestResult
            {
                durationMilliseconds = result.Duration.TotalMilliseconds.ToString("0"),
                fileName = Path.GetFileName(result.Class.Assembly.Location),
                outcome = "Passed",
                testFramework = "fixie",
                testName = result.Name
            });
        }

        public void CaseFailed(FailResult result)
        {
            Post(new TestResult
            {
                ErrorMessage = result.Exceptions.PrimaryException.Message,
                ErrorStackTrace = result.Exceptions.CompoundStackTrace,
                fileName = Path.GetFileName(result.Class.Assembly.Location),
                outcome = "Failed",
                testFramework = "fixie",
                testName = result.Name
            });
        }

        public void AssemblyCompleted(Assembly assembly, AssemblyResult result)
        {
        }

        void Post(TestResult result)
        {
            var content = new JavaScriptSerializer().Serialize(result);
            client.PostAsync(url, new StringContent(content, Encoding.UTF8, "application/json"))
                  .ContinueWith(x => x.Result.EnsureSuccessStatusCode())
                  .Wait();
        }

        private class TestResult
        {
            public string testName { get; set; }
            public string testFramework { get; set; }
            public string fileName { get; set; }
            public string outcome { get; set; }
            public string durationMilliseconds { get; set; }
            public string ErrorMessage { get; set; }
            public string ErrorStackTrace { get; set; }
        }
    }
}