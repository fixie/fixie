namespace Fixie.Internal.Listeners
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Reflection;
    using System.Runtime.Versioning;
    using System.Text;
    using System.Threading;
    using Internal;
    using static System.Environment;
    using static Serialization;
    using static System.Console;

    public class AzureListener :
        Handler<AssemblyStarted>,
        Handler<CaseSkipped>,
        Handler<CasePassed>,
        Handler<CaseFailed>,
        Handler<AssemblyCompleted>
    {
        const string AzureDevOpsRestApiVersion = "5.0";

        public delegate string ApiAction(HttpClient client, HttpMethod method, string uri, string mediaType, string content);

        readonly string collectionUri;
        readonly string project;
        readonly string buildId;
        readonly ApiAction send;
        readonly HttpClient client;

        string runUrl;

        readonly int batchSize;
        readonly List<Result> batch;
        bool apiUnavailable;

        public AzureListener()
            : this(
                GetEnvironmentVariable("SYSTEM_TEAMFOUNDATIONCOLLECTIONURI"),
                GetEnvironmentVariable("SYSTEM_TEAMPROJECT"),
                GetEnvironmentVariable("SYSTEM_ACCESSTOKEN"),
                GetEnvironmentVariable("BUILD_BUILDID"),
                Send,
                batchSize: 25)
        {
        }

        public AzureListener(string collectionUri, string project, string accessToken, string buildId, ApiAction send, int batchSize)
        {
            this.collectionUri = collectionUri;
            this.project = project;
            this.buildId = buildId;
            this.send = send;
            this.batchSize = batchSize;

            batch = new List<Result>(batchSize);

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

            var response = send(client, HttpMethod.Post, $"{runsUri}?api-version={AzureDevOpsRestApiVersion}", "application/json", Serialize(start));

            runUrl = Deserialize<TestRun>(response).url;
        }

        public void Handle(CaseSkipped message)
        {
            if (apiUnavailable) return;

            Include(message, x =>
            {
                x.outcome = "Warning";
                x.errorMessage = message.Reason;
            });
        }

        public void Handle(CasePassed message)
        {
            if (apiUnavailable) return;

            Include(message, x =>
            {
                x.outcome = "Passed";
            });
        }

        public void Handle(CaseFailed message)
        {
            if (apiUnavailable) return;

            Include(message, x =>
            {
                x.outcome = "Failed";
                x.errorMessage = message.Exception.Message;
                x.stackTrace =
                    message.Exception.TypeName() +
                    NewLine +
                    message.Exception.LiterateStackTrace();
            });
        }

        public void Handle(AssemblyCompleted message)
        {
            if (apiUnavailable) return;

            if (batch.Any())
                PostBatch();

            var finish = new UpdateRun
            {
                state = "Completed"
            };

            send(client, new HttpMethod("PATCH"), $"{runUrl}?api-version={AzureDevOpsRestApiVersion}", "application/json", Serialize(finish));
        }

        void Include(CaseCompleted message, Action<Result> customize)
        {
            var result = new Result
            {
                automatedTestName = message.Name,
                testCaseTitle = message.Name,
                durationInMs = message.Duration.TotalMilliseconds
            };

            customize(result);

            batch.Add(result);

            if (batch.Count >= batchSize)
                PostBatch();
        }

        int failuresToSimulate = 0;
        void PostBatch()
        {
            var attempt = 1;
            const int maxAttempts = 5;
            const int coolDownInSeconds = 5;

            while (attempt <= maxAttempts)
            {
                try
                {
                    var failureSuffix = attempt <= failuresToSimulate ? Guid.NewGuid().ToString() : "";

                    send(client, HttpMethod.Post, $"{runUrl}/results?api-version={AzureDevOpsRestApiVersion + failureSuffix}", "application/json", Serialize(batch));
                    batch.Clear();

                    if (attempt > 1)
                    {
                        WriteLine($"Successfully submitted test result batch to Azure DevOps API on attempt #{attempt}.");
                        WriteLine();
                    }

                    failuresToSimulate++;

                    return;
                }
                catch (Exception exception)
                {
                    WriteLine($"Failed to submit test result batch to Azure DevOps API (attempt #{attempt} of {maxAttempts}): " + exception);
                    WriteLine();
                    Thread.Sleep(TimeSpan.FromSeconds(coolDownInSeconds));
                    attempt++;
                }
            }

            WriteLine("Due to repeated failures while submitting test results to the Azure DevOps API,");
            WriteLine("further attempts will be suppressed for the remainder of this test run. Full test");
            WriteLine("results will continue to be reported to this console and to the test process exit");
            WriteLine("code, but the Azure DevOps \"Tests\" summary will be incomplete.");
            WriteLine();

            apiUnavailable = true;
            batch.Clear();
        }

        static string Send(HttpClient client, HttpMethod method, string uri, string mediaType, string content)
        {
            var task = client.SendAsync(
                new HttpRequestMessage(method, new Uri(uri, UriKind.RelativeOrAbsolute))
                {
                    Content = new StringContent(content, Encoding.UTF8, mediaType)
                });

            task.Wait();

            using (var httpResponse = task.Result)
            {
                var body = httpResponse.Content.ReadAsStringAsync().Result;

                if (!httpResponse.IsSuccessStatusCode)
                    throw new HttpRequestException(new StringBuilder()
                        .AppendLine($"{typeof(AzureListener).FullName} failed to {method} a message:")
                        .AppendLine($"{(int) httpResponse.StatusCode} {httpResponse.ReasonPhrase}")
                        .AppendLine(body)
                        .ToString());

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

        public class UpdateRun
        {
            public string state { get; set; }
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