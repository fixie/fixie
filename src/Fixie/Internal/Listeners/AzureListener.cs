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

    public class AzureListener :
        Handler<AssemblyStarted>,
        Handler<CaseSkipped>,
        Handler<CasePassed>,
        Handler<CaseFailed>
    {
        const string AzureDevOpsRestApiVersion = "5.0";

        public delegate string ApiAction(HttpClient client, string uri, string mediaType, string content);

        readonly string collectionUri;
        readonly string project;
        readonly string buildId;
        readonly ApiAction postAction;
        readonly HttpClient client;

        string runUrl;

        public AzureListener()
            : this(
                GetEnvironmentVariable("SYSTEM_TEAMFOUNDATIONCOLLECTIONURI"),
                GetEnvironmentVariable("SYSTEM_TEAMPROJECT"),
                GetEnvironmentVariable("SYSTEM_ACCESSTOKEN"),
                GetEnvironmentVariable("BUILD_BUILDID"),
                Post)
        {
        }

        public AzureListener(string collectionUri, string project, string accessToken, string buildId, ApiAction postAction)
        {
            this.collectionUri = collectionUri;
            this.project = project;
            this.buildId = buildId;
            this.postAction = postAction;
            
            client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        public void Handle(AssemblyStarted message)
        {
            var runName = Path.GetFileNameWithoutExtension(message.Assembly.Location);

            var framework = Assembly.GetEntryAssembly()?
                .GetCustomAttribute<TargetFrameworkAttribute>()?
                .FrameworkName;

            if (!string.IsNullOrEmpty(framework))
                runName = $"{runName} ({framework})";
            
            var start = new CreateRun
            {
                name = runName,
                build = new CreateRun.BuildDetail
                {
                    id = buildId
                },
                isAutomated = true
            };

            var runsUri = new Uri(new Uri(collectionUri), $"{project}/_apis/test/runs").ToString();

            var response = postAction(client, $"{runsUri}?api-version={AzureDevOpsRestApiVersion}", "application/json", Serialize(start));

            Console.WriteLine("Create Run Response:");
            Console.WriteLine(response);

            runUrl = Deserialize<TestRun>(response).url;
        }

        public void Handle(CaseSkipped message)
        {
            Post(message, x =>
            {
                x.outcome = "None";
                x.errorMessage = message.Reason;
            });
        }

        public void Handle(CasePassed message)
        {
            Post(message, x =>
            {
                x.outcome = "Passed";
            });
        }

        public void Handle(CaseFailed message)
        {
            Post(message, x =>
            {
                x.outcome = "Failed";
                x.errorMessage = message.Exception.Message;
                x.stackTrace =
                    message.Exception.TypeName() +
                    NewLine +
                    message.Exception.LiterateStackTrace();
            });
        }

        void Post(CaseCompleted message, Action<Result> customize)
        {
            var result = new Result
            {
                automatedTestName = message.Name,
                testCaseTitle = message.Name,
                durationInMs = message.Duration.TotalMilliseconds
            };

            customize(result);

            postAction(client, $"{runUrl}/results?api-version={AzureDevOpsRestApiVersion}", "application/json", Serialize(new[]{result}));
        }

        static string Post(HttpClient client, string uri, string mediaType, string content)
        {
            var task = client.PostAsync(uri, new StringContent(content, Encoding.UTF8, mediaType));
            
            task.Wait();

            using (var httpResponse = task.Result)
            {
                var body = httpResponse.Content.ReadAsStringAsync().Result;

                if (!httpResponse.IsSuccessStatusCode)
                {
                    Console.WriteLine($"{typeof(AzureListener).FullName} failed to POST a result: {(int)httpResponse.StatusCode} {httpResponse.ReasonPhrase}");
                    Console.WriteLine(body);
                }

                return body;
            }
        }

        public class CreateRun
        {
            public string name { get; set; }
            public BuildDetail build { get; set; }
            public bool isAutomated { get; set; }

            public class BuildDetail
            {
                public string id { get; set; }
            }
        }

        public class TestRun
        {
            public string url { get; set; }
        }

        public class Result
        {
            public string automatedTestName { get; set; }
            public string testCaseTitle { get; set; }
            public double durationInMs { get; set; }
            public string outcome { get; set; }
            public string errorMessage { get; set; }
            public string stackTrace { get; set; }
        }
    }
}