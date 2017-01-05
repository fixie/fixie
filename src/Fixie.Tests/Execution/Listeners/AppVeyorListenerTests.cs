namespace Fixie.Tests.Execution.Listeners
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Script.Serialization;
    using Fixie.Execution.Listeners;
    using Fixie.Internal;
    using Should;
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

                results[0].testName.ShouldEqual(testClass + ".SkipWithReason");
                results[0].outcome.ShouldEqual("Skipped");
                results[0].durationMilliseconds.ShouldEqual("0");
                results[0].ErrorMessage.ShouldEqual("Skipped with reason.");
                results[0].ErrorStackTrace.ShouldBeNull();
                results[0].StdOut.ShouldBeNull();

                results[1].testName.ShouldEqual(testClass + ".SkipWithoutReason");
                results[1].outcome.ShouldEqual("Skipped");
                results[1].durationMilliseconds.ShouldEqual("0");
                results[1].ErrorMessage.ShouldBeNull();
                results[1].ErrorStackTrace.ShouldBeNull();
                results[1].StdOut.ShouldBeNull();

                results[2].testName.ShouldEqual(testClass + ".Fail");
                results[2].outcome.ShouldEqual("Failed");
                int.Parse(results[2].durationMilliseconds).ShouldBeGreaterThanOrEqualTo(0);
                results[2].ErrorMessage.ShouldEqual("'Fail' failed!");
                results[2].ErrorStackTrace.Lines().Select(CleanBrittleValues)
                    .ShouldEqual(
                         "Fixie.Tests.FailureException",
                         "'Fail' failed!",
                         At<SampleTestClass>("Fail()"));
                results[2].StdOut.Lines().ShouldEqual("Console.Out: Fail", "Console.Error: Fail");

                results[3].testName.ShouldEqual(testClass + ".FailByAssertion");
                results[3].outcome.ShouldEqual("Failed");
                int.Parse(results[3].durationMilliseconds).ShouldBeGreaterThanOrEqualTo(0);
                results[3].ErrorMessage.Lines().ShouldEqual(
                    "Assert.Equal() Failure",
                    "Expected: 2",
                    "Actual:   1");
                results[3].ErrorStackTrace.Lines().Select(CleanBrittleValues)
                    .ShouldEqual(
                         "Should.Core.Exceptions.EqualException",
                         "Assert.Equal() Failure",
                         "Expected: 2",
                         "Actual:   1",
                         At<SampleTestClass>("FailByAssertion()"));
                results[3].StdOut.Lines().ShouldEqual("Console.Out: FailByAssertion", "Console.Error: FailByAssertion");

                results[4].testName.ShouldEqual(testClass + ".Pass");
                results[4].outcome.ShouldEqual("Passed");
                int.Parse(results[4].durationMilliseconds).ShouldBeGreaterThanOrEqualTo(0);
                results[4].ErrorMessage.ShouldBeNull();
                results[4].ErrorStackTrace.ShouldBeNull();
                results[4].StdOut.Lines().ShouldEqual("Console.Out: Pass", "Console.Error: Pass");
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