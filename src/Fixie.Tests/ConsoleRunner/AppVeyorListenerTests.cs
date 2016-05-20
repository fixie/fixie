namespace Fixie.Tests.ConsoleRunner
{
    using Should;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Script.Serialization;
    using Fixie.ConsoleRunner;
    using Fixie.Internal;
    using static Utility;

    public class AppVeyorListenerTests
    {
        public void ShouldReportResultsToAppVeyorBuildWorkerApi()
        {
            var results = new List<AppVeyorListener.TestResult>();

            var httpClient = new HttpClient(new FakeHandler(request =>
            {
                request.ShouldNotBeNull();
                request.RequestUri.AbsoluteUri.ShouldEqual("http://localhost:4567/api/tests");
                request.Headers.Accept.ShouldContain(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Content.Headers.ContentType.ToString().ShouldEqual("application/json; charset=utf-8");

                var requestContent = request.Content.ReadAsStringAsync().Result;
                results.Add(new JavaScriptSerializer().Deserialize<AppVeyorListener.TestResult>(requestContent));

                return new HttpResponseMessage { StatusCode = HttpStatusCode.Accepted };
            }));

            using (var console = new RedirectedConsole())
            {
                var listener = new AppVeyorListener("http://localhost:4567", httpClient);
                var convention = SampleTestClassConvention.Build();

                typeof(SampleTestClass).Run(listener, convention);

                var testClass = FullName<SampleTestClass>();

                console.Lines()
                    .ShouldEqual(
                        "Console.Out: Fail",
                        "Console.Error: Fail",
                        "Console.Out: FailByAssertion",
                        "Console.Error: FailByAssertion",
                        "Console.Out: Pass",
                        "Console.Error: Pass");

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

                skipWithReason.testName.ShouldEqual(testClass + ".SkipWithReason");
                skipWithReason.outcome.ShouldEqual("Skipped");
                skipWithReason.durationMilliseconds.ShouldEqual("0");
                skipWithReason.ErrorMessage.ShouldEqual("Skipped with reason.");
                skipWithReason.ErrorStackTrace.ShouldBeNull();
                skipWithReason.StdOut.ShouldBeNull();

                skipWithoutReason.testName.ShouldEqual(testClass + ".SkipWithoutReason");
                skipWithoutReason.outcome.ShouldEqual("Skipped");
                skipWithoutReason.durationMilliseconds.ShouldEqual("0");
                skipWithoutReason.ErrorMessage.ShouldBeNull();
                skipWithoutReason.ErrorStackTrace.ShouldBeNull();
                skipWithoutReason.StdOut.ShouldBeNull();

                fail.testName.ShouldEqual(testClass + ".Fail");
                fail.outcome.ShouldEqual("Failed");
                int.Parse(fail.durationMilliseconds).ShouldBeGreaterThanOrEqualTo(0);
                fail.ErrorMessage.ShouldEqual("'Fail' failed!");
                fail.ErrorStackTrace.Lines().Select(CleanBrittleValues)
                    .ShouldEqual(At<SampleTestClass>("Fail()"));
                fail.StdOut.Lines().ShouldEqual("Console.Out: Fail", "Console.Error: Fail");

                failByAssertion.testName.ShouldEqual(testClass + ".FailByAssertion");
                failByAssertion.outcome.ShouldEqual("Failed");
                int.Parse(failByAssertion.durationMilliseconds).ShouldBeGreaterThanOrEqualTo(0);
                failByAssertion.ErrorMessage.Lines().ShouldEqual(
                    "Assert.Equal() Failure",
                    "Expected: 2",
                    "Actual:   1");
                failByAssertion.ErrorStackTrace.Lines().Select(CleanBrittleValues)
                    .ShouldEqual(At<SampleTestClass>("FailByAssertion()"));
                failByAssertion.StdOut.Lines().ShouldEqual("Console.Out: FailByAssertion", "Console.Error: FailByAssertion");

                pass.testName.ShouldEqual(testClass + ".Pass");
                pass.outcome.ShouldEqual("Passed");
                int.Parse(pass.durationMilliseconds).ShouldBeGreaterThanOrEqualTo(0);
                pass.ErrorMessage.ShouldBeNull();
                pass.ErrorStackTrace.ShouldBeNull();
                pass.StdOut.Lines().ShouldEqual("Console.Out: Pass", "Console.Error: Pass");
            }
        }

        static string CleanBrittleValues(string actualRawContent)
        {
            //Avoid brittle assertion introduced by stack trace line numbers.
            var cleaned = Regex.Replace(actualRawContent, @":line \d+", ":line #");

            return cleaned;
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