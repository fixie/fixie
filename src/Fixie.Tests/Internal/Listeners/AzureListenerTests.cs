namespace Fixie.Tests.Internal.Listeners
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Assertions;
    using Fixie.Internal;
    using Fixie.Internal.Listeners;
    using static Fixie.Internal.Serialization;

    public class AzureListenerTests : MessagingTests
    {
        public void ShouldReportResultsToAzureDevOpsApi()
        {
            AzureListener.CreateRun start = null;
            var results = new List<AzureListener.Result>();
            var project = Guid.NewGuid().ToString();
            var accessToken = Guid.NewGuid().ToString();
            var buildId = Guid.NewGuid().ToString();
            var runUrl = "http://localhost:4567/run/" + Guid.NewGuid();

            bool first = true;
            var listener = new AzureListener("http://localhost:4567", project, accessToken, buildId, (client, uri, mediaType, content) =>
            {
                var actualHeader = client.DefaultRequestHeaders.Accept.Single();
                actualHeader.MediaType.ShouldBe("application/json");

                var actualAuthorization = client.DefaultRequestHeaders.Authorization;
                actualAuthorization.Scheme.ShouldBe("Bearer");
                actualAuthorization.Parameter.ShouldBe(accessToken);

                mediaType.ShouldBe("application/json");

                if (first)
                {
                    first = false;

                    uri.ShouldBe($"http://localhost:4567/{project}/_apis/test/runs?api-version=5.0");

                    start = Deserialize<AzureListener.CreateRun>(content);

                    return Serialize(new AzureListener.TestRun { url = runUrl });
                }

                uri.ShouldBe($"{runUrl}/results?api-version=5.0");

                results.Add(Deserialize<AzureListener.Result[]>(content).Single());

                return null;
            });

            using (var console = new RedirectedConsole())
            {
                Run(listener);

                console.Lines()
                    .ShouldBe(
                        "Console.Out: Fail",
                        "Console.Error: Fail",
                        "Console.Out: FailByAssertion",
                        "Console.Error: FailByAssertion",
                        "Console.Out: Pass",
                        "Console.Error: Pass");
            }

            

#if NET452
            start.name.ShouldBe("Fixie.Tests (.NETFramework,Version=v4.5.2)");
#else
            start.name.ShouldBe("Fixie.Tests (.NETCoreApp,Version=v2.0)");
#endif

            start.build.id.ShouldBe(buildId);
            start.isAutomated.ShouldBe(true);

            results.Count.ShouldBe(5);

            var fail = results[0];
            var failByAssertion = results[1];
            var pass = results[2];
            var skipWithReason = results[3];
            var skipWithoutReason = results[4];

            skipWithReason.automatedTestName.ShouldBe(TestClass + ".SkipWithReason");
            skipWithReason.testCaseTitle.ShouldBe(TestClass + ".SkipWithReason");
            skipWithReason.outcome.ShouldBe("None");
            skipWithReason.durationInMs.ShouldBeGreaterThanOrEqualTo(0);
            skipWithReason.errorMessage.ShouldBe("⚠ Skipped with reason.");
            skipWithReason.stackTrace.ShouldBe(null);

            skipWithoutReason.automatedTestName.ShouldBe(TestClass + ".SkipWithoutReason");
            skipWithoutReason.testCaseTitle.ShouldBe(TestClass + ".SkipWithoutReason");
            skipWithoutReason.outcome.ShouldBe("None");
            skipWithoutReason.durationInMs.ShouldBeGreaterThanOrEqualTo(0);
            skipWithoutReason.errorMessage.ShouldBe(null);
            skipWithoutReason.stackTrace.ShouldBe(null);

            fail.automatedTestName.ShouldBe(TestClass + ".Fail");
            fail.testCaseTitle.ShouldBe(TestClass + ".Fail");
            fail.outcome.ShouldBe("Failed");
            fail.durationInMs.ShouldBeGreaterThanOrEqualTo(0);
            fail.errorMessage.ShouldBe("'Fail' failed!");
            fail.stackTrace
                .CleanStackTraceLineNumbers()
                .Lines()
                .ShouldBe("Fixie.Tests.FailureException", At("Fail()"));

            failByAssertion.automatedTestName.ShouldBe(TestClass + ".FailByAssertion");
            failByAssertion.testCaseTitle.ShouldBe(TestClass + ".FailByAssertion");
            failByAssertion.outcome.ShouldBe("Failed");
            failByAssertion.durationInMs.ShouldBeGreaterThanOrEqualTo(0);
            failByAssertion.errorMessage.Lines().ShouldBe(
                "Expected: 2",
                "Actual:   1");
            failByAssertion.stackTrace
                .CleanStackTraceLineNumbers()
                .Lines()
                .ShouldBe("Fixie.Tests.Assertions.AssertException", At("FailByAssertion()"));

            pass.automatedTestName.ShouldBe(TestClass + ".Pass");
            pass.testCaseTitle.ShouldBe(TestClass + ".Pass");
            pass.outcome.ShouldBe("Passed");
            pass.durationInMs.ShouldBeGreaterThanOrEqualTo(0);
            pass.errorMessage.ShouldBe(null);
            pass.stackTrace.ShouldBe(null);
        }
    }
}