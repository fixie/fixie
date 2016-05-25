namespace Fixie.Tests.ConsoleRunner
{
    using Should;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Script.Serialization;
    using Fixie.ConsoleRunner;
    using Fixie.Internal;

    public class AppVeyorListenerTests : MessagingTests
    {
        public void ShouldReportResultsToAppVeyorBuildWorkerApi()
        {
            var results = new List<AppVeyorListener.TestResult>();

            using (var httpClient = FakeHttpClient(results))
            {
                var listener = new AppVeyorListener("http://localhost:4567", httpClient);

                using (var console = new RedirectedConsole())
                {
                    Run(listener);

                    console.Lines()
                        .ShouldEqual(
                            "Console.Out: Fail",
                            "Console.Error: Fail",
                            "Console.Out: FailByAssertion",
                            "Console.Error: FailByAssertion",
                            "Console.Out: Pass",
                            "Console.Error: Pass");
                }
            }

            results.Count.ShouldEqual(5);

            foreach (var result in results)
            {
                result.testFramework.ShouldEqual("Fixie");
                result.fileName.ShouldEqual("Fixie.Tests.dll");
            }

            var skipWithReason = results[0];
            var skipWithoutReason = results[1];
            var fail = results[2];
            var failByAssertion = results[3];
            var pass = results[4];

            skipWithReason.testName.ShouldEqual(TestClass + ".SkipWithReason");
            skipWithReason.outcome.ShouldEqual("Skipped");
            skipWithReason.durationMilliseconds.ShouldEqual("0");
            skipWithReason.ErrorMessage.ShouldEqual("Skipped with reason.");
            skipWithReason.ErrorStackTrace.ShouldBeNull();
            skipWithReason.StdOut.ShouldBeNull();

            skipWithoutReason.testName.ShouldEqual(TestClass + ".SkipWithoutReason");
            skipWithoutReason.outcome.ShouldEqual("Skipped");
            skipWithoutReason.durationMilliseconds.ShouldEqual("0");
            skipWithoutReason.ErrorMessage.ShouldBeNull();
            skipWithoutReason.ErrorStackTrace.ShouldBeNull();
            skipWithoutReason.StdOut.ShouldBeNull();

            fail.testName.ShouldEqual(TestClass + ".Fail");
            fail.outcome.ShouldEqual("Failed");
            int.Parse(fail.durationMilliseconds).ShouldBeGreaterThanOrEqualTo(0);
            fail.ErrorMessage.ShouldEqual("'Fail' failed!");
            fail.ErrorStackTrace
                .CleanStackTraceLineNumbers()
                .ShouldEqual(At("Fail()"));
            fail.StdOut.Lines().ShouldEqual("Console.Out: Fail", "Console.Error: Fail");

            failByAssertion.testName.ShouldEqual(TestClass + ".FailByAssertion");
            failByAssertion.outcome.ShouldEqual("Failed");
            int.Parse(failByAssertion.durationMilliseconds).ShouldBeGreaterThanOrEqualTo(0);
            failByAssertion.ErrorMessage.Lines().ShouldEqual(
                "Assert.Equal() Failure",
                "Expected: 2",
                "Actual:   1");
            failByAssertion.ErrorStackTrace
                .CleanStackTraceLineNumbers()
                .ShouldEqual(At("FailByAssertion()"));
            failByAssertion.StdOut.Lines().ShouldEqual("Console.Out: FailByAssertion", "Console.Error: FailByAssertion");

            pass.testName.ShouldEqual(TestClass + ".Pass");
            pass.outcome.ShouldEqual("Passed");
            int.Parse(pass.durationMilliseconds).ShouldBeGreaterThanOrEqualTo(0);
            pass.ErrorMessage.ShouldBeNull();
            pass.ErrorStackTrace.ShouldBeNull();
            pass.StdOut.Lines().ShouldEqual("Console.Out: Pass", "Console.Error: Pass");
        }

        static HttpClient FakeHttpClient(List<AppVeyorListener.TestResult> results)
        {
            return new HttpClient(new FakeHandler(request =>
            {
                request.ShouldNotBeNull();
                request.RequestUri.AbsoluteUri.ShouldEqual("http://localhost:4567/api/tests");
                request.Headers.Accept.ShouldContain(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Content.Headers.ContentType.ToString().ShouldEqual("application/json; charset=utf-8");

                var requestContent1 = request.Content.ReadAsStringAsync().Result;
                results.Add(new JavaScriptSerializer().Deserialize<AppVeyorListener.TestResult>(requestContent1));

                return new HttpResponseMessage { StatusCode = HttpStatusCode.Accepted };
            }));
        }

        class FakeHandler : DelegatingHandler
        {
            readonly Func<HttpRequestMessage, HttpResponseMessage> func;

            public FakeHandler(Func<HttpRequestMessage, HttpResponseMessage> func)
            {
                this.func = func;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(func(request));
            }
        }
    }
}