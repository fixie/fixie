
using Fixie.Execution;
using Fixie.Listeners;
using Should;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Fixie.Tests.Listeners
{
    public class AppVeyorListenerTests
    {
        public void ShouldReportFailedTests()
        {
            var convention = SelfTestConvention.Build();

            HttpRequestMessage request = null;
            string content = null;
            var listener = new AppVeyorListener("http://localhost:4567",
                                                new HttpClient(new FakeHandler(x =>
                                                {
                                                    request = x;
                                                    content = request.Content.ReadAsStringAsync().Result;
                                                    return new HttpResponseMessage { StatusCode = HttpStatusCode.Accepted };
                                                })));

            var runner = new ClassRunner(listener, convention.Config);
            runner.Run(typeof(FailTestClass));

            request.ShouldNotBeNull();
            request.RequestUri.AbsoluteUri.ShouldEqual("http://localhost:4567/api/tests");
            request.Headers.Accept.ShouldContain(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content.Headers.ContentType.ToString().ShouldEqual("application/json; charset=utf-8");

            var result = new JavaScriptSerializer().Deserialize<TestResult>(content);
            result.ErrorMessage.ShouldEqual("'Fail' failed!");
            result.ErrorStackTrace
                  .Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                  .Select(x => Regex.Replace(x, @":line \d+", ":line #")) //Avoid brittle assertion introduced by stack trace line numbers.
                  .ShouldEqual(
                      "'Fail' failed!",
                      "   at Fixie.Tests.Listeners.AppVeyorListenerTests.FailTestClass.Fail() in " + PathToThisFile() + ":line #");
            result.durationMilliseconds.ShouldBeNull();
            result.fileName.ShouldNotBeEmpty();
            result.outcome.ShouldEqual("Failed");
            result.testFramework.ShouldEqual("fixie");
            result.testName.ShouldEqual("Fixie.Tests.Listeners.AppVeyorListenerTests+FailTestClass.Fail");
        }

        static string PathToThisFile([CallerFilePath] string path = null)
        {
            return path;
        }

        public void ShouldReportPassedTests()
        {
            var convention = SelfTestConvention.Build();

            HttpRequestMessage request = null;
            string content = null;
            var listener = new AppVeyorListener("http://localhost:4567",
                                                new HttpClient(new FakeHandler(x =>
                                                {
                                                    request = x;
                                                    content = request.Content.ReadAsStringAsync().Result;
                                                    return new HttpResponseMessage { StatusCode = HttpStatusCode.Accepted };
                                                })));

            var runner = new ClassRunner(listener, convention.Config);
            runner.Run(typeof(PassTestClass));

            request.ShouldNotBeNull();
            request.RequestUri.AbsoluteUri.ShouldEqual("http://localhost:4567/api/tests");
            request.Headers.Accept.ShouldContain(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content.Headers.ContentType.ToString().ShouldEqual("application/json; charset=utf-8");

            var result = new JavaScriptSerializer().Deserialize<TestResult>(content);
            result.ErrorMessage.ShouldBeNull();
            result.ErrorStackTrace.ShouldBeNull();
            Regex.IsMatch(result.durationMilliseconds, @"\d+").ShouldBeTrue();
            result.fileName.ShouldNotBeEmpty();
            result.outcome.ShouldEqual("Passed");
            result.testFramework.ShouldEqual("fixie");
            result.testName.ShouldEqual("Fixie.Tests.Listeners.AppVeyorListenerTests+PassTestClass.Pass");
        }

        public void ShouldReportSkippedTests()
        {
            var convention = SelfTestConvention.Build();
            convention.CaseExecution.Skip(@case => @case.Method.Name == "Skip");

            HttpRequestMessage request = null;
            string content = null;
            var listener = new AppVeyorListener("http://localhost:4567",
                                                new HttpClient(new FakeHandler(x =>
                                                {
                                                    request = x;
                                                    content = request.Content.ReadAsStringAsync().Result;
                                                    return new HttpResponseMessage { StatusCode = HttpStatusCode.Accepted };
                                                })));

            var runner = new ClassRunner(listener, convention.Config);
            runner.Run(typeof(SkipTestClass));

            request.ShouldNotBeNull();
            request.RequestUri.AbsoluteUri.ShouldEqual("http://localhost:4567/api/tests");
            request.Headers.Accept.ShouldContain(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content.Headers.ContentType.ToString().ShouldEqual("application/json; charset=utf-8");

            var result = new JavaScriptSerializer().Deserialize<TestResult>(content);
            result.ErrorMessage.ShouldBeNull();
            result.ErrorStackTrace.ShouldBeNull();
            result.durationMilliseconds.ShouldBeNull();
            result.fileName.ShouldNotBeEmpty();
            result.outcome.ShouldEqual("Skipped");
            result.testFramework.ShouldEqual("fixie");
            result.testName.ShouldEqual("Fixie.Tests.Listeners.AppVeyorListenerTests+SkipTestClass.Skip");
        }

        private class PassTestClass
        {
            public void Pass() { }
        }

        private class FailTestClass
        {
            public void Fail()
            {
                throw new FailureException();
            }
        }

        private class SkipTestClass
        {
            public void Skip()
            {
                throw new ShouldBeUnreachableException();
            }
        }

        private class FakeHandler : DelegatingHandler
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