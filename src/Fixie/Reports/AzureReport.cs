﻿namespace Fixie.Reports
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Internal;
    using static System.Environment;
    using static Internal.Serialization;
    using static Internal.Maybe;

    class AzureReport :
        IHandler<ExecutionStarted>,
        IHandler<TestSkipped>,
        IHandler<TestPassed>,
        IHandler<TestFailed>,
        IHandler<ExecutionCompleted>
    {
        const string AzureDevOpsRestApiVersion = "5.0";

        public delegate Task<string> ApiAction<in T>(HttpClient client, HttpMethod method, string uri, T content);

        readonly TextWriter console;
        readonly TestEnvironment environment;
        readonly string collectionUri;
        readonly string project;
        readonly string buildId;
        readonly ApiAction<CreateRun> sendCreateRun;
        readonly ApiAction<IReadOnlyList<Result>> sendResultsBatch;
        readonly ApiAction<CompleteRun> sendCompleteRun;
        readonly HttpClient client;

        string? runUrl;

        readonly int batchSize;
        readonly List<Result> batch;
        bool apiUnavailable;

        internal static AzureReport? Create(TestEnvironment environment)
        {
            var console = environment.Console;
            var runningUnderAzure = GetEnvironmentVariable("TF_BUILD") == "True";

            if (runningUnderAzure)
            {
                var accessTokenIsAvailable =
                    !string.IsNullOrEmpty(GetEnvironmentVariable("SYSTEM_ACCESSTOKEN"));

                if (accessTokenIsAvailable)
                {
                    if (TryGetEnvironmentVariable("SYSTEM_TEAMFOUNDATIONCOLLECTIONURI", console, out var collectionUri)
                        && TryGetEnvironmentVariable("SYSTEM_TEAMPROJECT", console, out var project)
                        && TryGetEnvironmentVariable("SYSTEM_ACCESSTOKEN", console, out var accessToken)
                        && TryGetEnvironmentVariable("BUILD_BUILDID", console, out var buildId))
                    {
                        return new AzureReport(
                            environment,
                            collectionUri,
                            project,
                            accessToken,
                            buildId,
                            Send,
                            Send,
                            Send,
                            batchSize: 25);
                    }

                    return null;
                }

                using (Foreground.Yellow)
                {
                    console.WriteLine("The Azure DevOps access token has not been made available to this process, so");
                    console.WriteLine("test results will not be collected. To resolve this issue, review your pipeline");
                    console.WriteLine("definition to ensure that the access token is made available as the environment");
                    console.WriteLine("variable SYSTEM_ACCESSTOKEN.");
                    console.WriteLine();
                    console.WriteLine("From https://docs.microsoft.com/en-us/azure/devops/pipelines/build/variables#systemaccesstoken");
                    console.WriteLine();
                    console.WriteLine("  env:");
                    console.WriteLine("    SYSTEM_ACCESSTOKEN: $(System.AccessToken)");
                    console.WriteLine();
                }
            }

            return null;
        }

        static bool TryGetEnvironmentVariable(string variable, TextWriter console, [NotNullWhen(true)] out string? value)
        {
            if (Try(GetEnvironmentVariable, variable, out value))
                return true;
            
            using (Foreground.Yellow)
            {
                console.WriteLine($"The Azure DevOps environment variable '{variable}' has not been made");
                console.WriteLine("available to this process, so test results will not be collected.");
                console.WriteLine();
            }

            return false;
        }

        public AzureReport(
            TestEnvironment environment,
            string collectionUri,
            string project,
            string accessToken,
            string buildId,
            ApiAction<CreateRun> sendCreateRun,
            ApiAction<IReadOnlyList<Result>> sendResultsBatch,
            ApiAction<CompleteRun> sendCompleteRun,
            int batchSize)
        {
            this.environment = environment;
            console = environment.Console;
            this.collectionUri = collectionUri;
            this.project = project;
            this.buildId = buildId;
            this.sendCreateRun = sendCreateRun;
            this.sendResultsBatch = sendResultsBatch;
            this.sendCompleteRun = sendCompleteRun;
            this.batchSize = batchSize;

            batch = new List<Result>(batchSize);

            client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        public async Task Handle(ExecutionStarted message)
        {
            var runName = Path.GetFileNameWithoutExtension(environment.Assembly.Location);

            var framework = environment.TargetFramework;

            if (!string.IsNullOrEmpty(framework))
                runName = $"{runName} ({framework})";

            var createRun = new CreateRun(runName, buildId);

            var runsUri = new Uri(new Uri(collectionUri), $"{project}/_apis/test/runs").ToString();

            var response = await sendCreateRun(client, HttpMethod.Post, $"{runsUri}?api-version={AzureDevOpsRestApiVersion}", createRun);

            runUrl = Deserialize<TestRun>(response).url;
        }

        public async Task Handle(TestSkipped message)
        {
            if (apiUnavailable) return;

            await Include(new Result(message, "Warning")
            {
                errorMessage = message.Reason
            });
        }

        public async Task Handle(TestPassed message)
        {
            if (apiUnavailable) return;

            await Include(new Result(message, "Passed"));
        }

        public async Task Handle(TestFailed message)
        {
            if (apiUnavailable) return;

            await Include(new Result(message, "Failed")
            {
                errorMessage = message.Reason.Message,
                stackTrace =
                    message.Reason.GetType().FullName +
                    NewLine +
                    message.Reason.LiterateStackTrace()
            });
        }

        public async Task Handle(ExecutionCompleted message)
        {
            if (apiUnavailable) return;

            if (batch.Any())
                await PostBatch();

            var completeRun = new CompleteRun();

            await sendCompleteRun(client, new HttpMethod("PATCH"), $"{runUrl}?api-version={AzureDevOpsRestApiVersion}", completeRun);
        }

        async Task Include(Result result)
        {
            batch.Add(result);

            if (batch.Count >= batchSize)
                await PostBatch();
        }

        async Task PostBatch()
        {
            var attempt = 1;
            const int maxAttempts = 5;
            const int coolDownInSeconds = 5;

            while (attempt <= maxAttempts)
            {
                try
                {
                    await sendResultsBatch(client, HttpMethod.Post, $"{runUrl}/results?api-version={AzureDevOpsRestApiVersion}", batch.ToList());
                    batch.Clear();

                    if (attempt > 1)
                    {
                        await console.WriteLineAsync($"Successfully submitted test result batch to Azure DevOps API on attempt #{attempt}.");
                        await console.WriteLineAsync();
                    }

                    return;
                }
                catch (Exception exception)
                {
                    await console.WriteLineAsync($"Failed to submit test result batch to Azure DevOps API (attempt #{attempt} of {maxAttempts}): " + exception);
                    await console.WriteLineAsync();
                    Thread.Sleep(TimeSpan.FromSeconds(coolDownInSeconds));
                    attempt++;
                }
            }

            await console.WriteLineAsync("Due to repeated failures while submitting test results to the Azure DevOps API,");
            await console.WriteLineAsync("further attempts will be suppressed for the remainder of this test run. Full test");
            await console.WriteLineAsync("results will continue to be reported to this console and to the test process exit");
            await console.WriteLineAsync("code, but the Azure DevOps \"Tests\" summary will be incomplete.");
            await console.WriteLineAsync();

            apiUnavailable = true;
            batch.Clear();
        }

        static async Task<string> Send<T>(HttpClient client, HttpMethod method, string uri, T content)
        {
            var serialized = Serialize(content);

            using var httpResponse = await client.SendAsync(
                new HttpRequestMessage(method, new Uri(uri, UriKind.RelativeOrAbsolute))
                {
                    Content = new StringContent(serialized, Encoding.UTF8, "application/json")
                });

            var body = await httpResponse.Content.ReadAsStringAsync();

            if (!httpResponse.IsSuccessStatusCode)
                throw new HttpRequestException(new StringBuilder()
                    .AppendLine($"{typeof(AzureReport).FullName} failed to {method} a message:")
                    .AppendLine($"{(int) httpResponse.StatusCode} {httpResponse.ReasonPhrase}")
                    .AppendLine(body)
                    .ToString());

            return body;
        }

        public class CreateRun
        {
            public CreateRun(string runName, string buildId)
            {
                name = runName;
                build = new BuildDetail(buildId);
            }

            public string name { get; }
            public BuildDetail build { get; }
            public bool isAutomated => true;

            public class BuildDetail
            {
                public BuildDetail(string buildId) => id = buildId;

                public string id { get; }
            }
        }

        public class CompleteRun
        {
            public string state => "Completed";
        }

        public class TestRun
        {
            public string? url { get; set; }
        }

        public class Result
        {
            public Result(TestCompleted message, string outcome)
            {
                automatedTestName = message.TestCase;
                testCaseTitle = message.TestCase;
                durationInMs = message.Duration.TotalMilliseconds;
                this.outcome = outcome;
            }

            public string automatedTestName { get; }
            public string testCaseTitle { get; }
            public double durationInMs { get; }
            public string outcome { get; set; }
            public string? errorMessage { get; set; }
            public string? stackTrace { get; set; }
        }
    }
}