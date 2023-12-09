namespace Fixie.Tests.Reports;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Assertions;
using Fixie.Reports;
using static Utility;
using static System.Text.Json.JsonSerializer;

public class AzureReportTests : MessagingTests
{
    class Request<TContent>
    {
        public Request(HttpMethod method, string uri, TContent content)
        {
            Method = method;
            Uri = uri;
            Content = content;
        }

        public HttpMethod Method { get; }
        public string Uri { get; }
        public TContent Content { get; }
    }

    public async Task ShouldReportResultsToAzureDevOpsApi()
    {
        var project = Guid.NewGuid().ToString();
        var accessToken = Guid.NewGuid().ToString();
        var buildId = Guid.NewGuid().ToString();
        var runUrl = "http://localhost:4567/run/" + Guid.NewGuid();
        var requests = new List<object>();
        var batchSize = 3;

        Action<HttpClient> assertCommonHttpConcerns = client =>
        {
            var actualHeader = client.DefaultRequestHeaders.Accept.Single();
            actualHeader.MediaType.ShouldBe("application/json");

            var actualAuthorization = client.DefaultRequestHeaders.Authorization;
            actualAuthorization.ShouldNotBeNull();
            actualAuthorization.Scheme.ShouldBe("Bearer");
            actualAuthorization.Parameter.ShouldBe(accessToken);
        };

        var output = await Run(environment =>
            new AzureReport(environment, "http://localhost:4567", project, accessToken, buildId,
                (client, method, uri, content) =>
                {
                    assertCommonHttpConcerns(client);
                    requests.Add(new Request<AzureReport.CreateRun>(method, uri, content));
                    return Task.FromResult(Serialize(new AzureReport.TestRun {url = runUrl}));
                },
                (client, method, uri, content) =>
                {
                    assertCommonHttpConcerns(client);
                    requests.Add(new Request<IReadOnlyList<AzureReport.Result>>(method, uri, content));
                    return Task.FromResult("");
                },
                (client, method, uri, content) =>
                {
                    assertCommonHttpConcerns(client);
                    requests.Add(new Request<AzureReport.CompleteRun>(method, uri, content));
                    return Task.FromResult("");
                }, batchSize));

        output.Console
            .ShouldBe(
                "Standard Out: Fail",
                "Standard Out: FailByAssertion",
                "Standard Out: Pass");

        var firstRequest = (Request<AzureReport.CreateRun>)requests.First();
        firstRequest.Method.ShouldBe(HttpMethod.Post);
        firstRequest.Uri.ShouldBe($"http://localhost:4567/{project}/_apis/test/runs?api-version=5.0");

        var createRun = firstRequest.Content;
        createRun.name.ShouldBe($"Fixie.Tests (.NETCoreApp,Version=v{TargetFrameworkVersion})");
        createRun.build.id.ShouldBe(buildId);
        createRun.isAutomated.ShouldBe(true);

        var resultBatches = requests
            .Skip(1)
            .Take(requests.Count - 2)
            .Cast<Request<IReadOnlyList<AzureReport.Result>>>()
            .Select(request =>
            {
                request.Method.ShouldBe(HttpMethod.Post);
                request.Uri.ShouldBe($"{runUrl}/results?api-version=5.0");

                return request.Content;
            }).ToList();

        resultBatches.Count.ShouldBe(3);
        resultBatches[0].Count.ShouldBe(3);
        resultBatches[1].Count.ShouldBe(3);
        resultBatches[2].Count.ShouldBe(1);

        var results = resultBatches.SelectMany(x => x).ToList();
        results.Count.ShouldBe(7);

        var fail = results[0];
        var failByAssertion = results[1];
        var pass = results[2];
        var skip = results[3];
        var shouldBeStringPassA = results[4];
        var shouldBeStringPassB = results[5];
        var shouldBeStringFail = results[6];

        fail.automatedTestName.ShouldBe(TestClass + ".Fail");
        fail.testCaseTitle.ShouldBe(TestClass + ".Fail");
        fail.outcome.ShouldBe("Failed");
        fail.durationInMs.ShouldBeGreaterThanOrEqualTo(0);
        fail.errorMessage.ShouldBe("'Fail' failed!");
        fail.stackTrace
            .Lines()
            .NormalizeStackTraceLines()
            .ShouldBe("Fixie.Tests.FailureException", At("Fail()"));

        failByAssertion.automatedTestName.ShouldBe(TestClass + ".FailByAssertion");
        failByAssertion.testCaseTitle.ShouldBe(TestClass + ".FailByAssertion");
        failByAssertion.outcome.ShouldBe("Failed");
        failByAssertion.durationInMs.ShouldBeGreaterThanOrEqualTo(0);
        failByAssertion.errorMessage.Lines().ShouldBe(
            "Expected: 2",
            "Actual:   1");
        failByAssertion.stackTrace
            .Lines()
            .NormalizeStackTraceLines()
            .ShouldBe("Fixie.Tests.Assertions.AssertException", At("FailByAssertion()"));

        pass.automatedTestName.ShouldBe(TestClass + ".Pass");
        pass.testCaseTitle.ShouldBe(TestClass + ".Pass");
        pass.outcome.ShouldBe("Passed");
        pass.durationInMs.ShouldBeGreaterThanOrEqualTo(0);
        pass.errorMessage.ShouldBe(null);
        pass.stackTrace.ShouldBe(null);

        skip.automatedTestName.ShouldBe(TestClass + ".Skip");
        skip.testCaseTitle.ShouldBe(TestClass + ".Skip");
        skip.outcome.ShouldBe("Warning");
        skip.durationInMs.ShouldBeGreaterThanOrEqualTo(0);
        skip.errorMessage.ShouldBe("⚠ Skipped with attribute.");
        skip.stackTrace.ShouldBe(null);

        shouldBeStringPassA.automatedTestName.ShouldBe(GenericTestClass + ".ShouldBeString<System.String>(\"A\")");
        shouldBeStringPassA.testCaseTitle.ShouldBe(GenericTestClass + ".ShouldBeString<System.String>(\"A\")");
        shouldBeStringPassA.outcome.ShouldBe("Passed");
        shouldBeStringPassA.durationInMs.ShouldBeGreaterThanOrEqualTo(0);
        shouldBeStringPassA.errorMessage.ShouldBe(null);
        shouldBeStringPassA.stackTrace.ShouldBe(null);

        shouldBeStringPassB.automatedTestName.ShouldBe(GenericTestClass + ".ShouldBeString<System.String>(\"B\")");
        shouldBeStringPassB.testCaseTitle.ShouldBe(GenericTestClass + ".ShouldBeString<System.String>(\"B\")");
        shouldBeStringPassB.outcome.ShouldBe("Passed");
        shouldBeStringPassB.durationInMs.ShouldBeGreaterThanOrEqualTo(0);
        shouldBeStringPassB.errorMessage.ShouldBe(null);
        shouldBeStringPassB.stackTrace.ShouldBe(null);

        shouldBeStringFail.automatedTestName.ShouldBe(GenericTestClass + ".ShouldBeString<System.Int32>(123)");
        shouldBeStringFail.testCaseTitle.ShouldBe(GenericTestClass + ".ShouldBeString<System.Int32>(123)");
        shouldBeStringFail.outcome.ShouldBe("Failed");
        shouldBeStringFail.durationInMs.ShouldBeGreaterThanOrEqualTo(0);
        shouldBeStringFail.errorMessage.Lines().ShouldBe(
            "Expected: System.String",
            "Actual:   System.Int32");
        shouldBeStringFail.stackTrace
            .Lines()
            .NormalizeStackTraceLines()
            .ShouldBe(
                "Fixie.Tests.Assertions.AssertException",
                At<SampleGenericTestClass>("ShouldBeString[T](T genericArgument)"));

        var lastRequest = (Request<AzureReport.CompleteRun>)requests.Last();
        lastRequest.Method.ShouldBe(new HttpMethod("PATCH"));
        lastRequest.Uri.ShouldBe($"{runUrl}?api-version=5.0");

        var updateRun = lastRequest.Content;
        updateRun.state.ShouldBe("Completed");
    }
}