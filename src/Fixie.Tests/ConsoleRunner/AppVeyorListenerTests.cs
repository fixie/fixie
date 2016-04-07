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

namespace Fixie.Tests.ConsoleRunner
{
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
                var convention = SelfTestConvention.Build();
                convention.CaseExecution.Skip(x => x.Method.Has<SkipAttribute>(), x => x.Method.GetCustomAttribute<SkipAttribute>().Reason);
                convention.Parameters.Add<InputAttributeParameterSource>();

                typeof(PassFailTestClass).Run(listener, convention);

                var testClass = typeof(PassFailTestClass).FullName;

                console.Lines()
                    .ShouldEqual(
                        "Console.Out: Fail",
                        "Console.Error: Fail",
                        "Console.Out: Pass",
                        "Console.Error: Pass");

                results.Count.ShouldEqual(4);

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
                results[2].ErrorMessage.ShouldEqual("Fixie.Tests.FailureException");
                results[2].ErrorStackTrace.Lines().Select(CleanBrittleValues)
                    .ShouldEqual("'Fail' failed!",
                         "   at Fixie.Tests.ConsoleRunner.AppVeyorListenerTests.PassFailTestClass.Fail() in " + PathToThisFile() + ":line #");
                results[2].StdOut.Lines().ShouldEqual("Console.Out: Fail", "Console.Error: Fail");

                results[3].testName.ShouldEqual(testClass + ".Pass(123)");
                results[3].outcome.ShouldEqual("Passed");
                int.Parse(results[3].durationMilliseconds).ShouldBeGreaterThanOrEqualTo(0);
                results[3].ErrorMessage.ShouldBeNull();
                results[3].ErrorStackTrace.ShouldBeNull();
                results[3].StdOut.Lines().ShouldEqual("Console.Out: Pass", "Console.Error: Pass");
            }
        }

        static string CleanBrittleValues(string actualRawContent)
        {
            //Avoid brittle assertion introduced by stack trace line numbers.
            var cleaned = Regex.Replace(actualRawContent, @":line \d+", ":line #");

            return cleaned;
        }

        static string PathToThisFile([CallerFilePath] string path = null)
        {
            return path;
        }

        class PassFailTestClass
        {
            [Input(123)]
            public void Pass(int x)
            {
                WhereAmI();
            }

            public void Fail()
            {
                WhereAmI();
                throw new FailureException();
            }

            [Skip]
            public void SkipWithoutReason() { throw new ShouldBeUnreachableException(); }

            [Skip("Skipped with reason.")]
            public void SkipWithReason() { throw new ShouldBeUnreachableException(); }

            static void WhereAmI([CallerMemberName] string member = null)
            {
                Console.Out.WriteLine("Console.Out: " + member);
                Console.Error.WriteLine("Console.Error: " + member);
            }
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