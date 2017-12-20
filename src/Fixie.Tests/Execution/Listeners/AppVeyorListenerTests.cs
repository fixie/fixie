namespace Fixie.Tests.Execution.Listeners
{
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization.Json;
    using System.Text;
    using Assertions;
    using Fixie.Execution;
    using Fixie.Execution.Listeners;

    public class AppVeyorListenerTests : MessagingTests
    {
        public void ShouldReportResultsToAppVeyorBuildWorkerApi()
        {
            var results = new List<AppVeyorListener.TestResult>();

            var listener = new AppVeyorListener("http://localhost:4567", (uri, mediaType, content) =>
            {
                uri.ShouldEqual("http://localhost:4567/api/tests");
                mediaType.ShouldEqual("application/json");

                results.Add(Deserialize(content));
            });

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

            results.Count.ShouldEqual(5);

            foreach (var result in results)
            {
                result.testFramework.ShouldEqual("Fixie");

#if NET471
                result.fileName.ShouldEqual("Fixie.Tests.exe");
#else
                result.fileName.ShouldEqual("Fixie.Tests.dll");
#endif
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
                .Lines()
                .ShouldEqual("Fixie.Tests.FailureException", At("Fail()"));
            fail.StdOut.Lines().ShouldEqual("Console.Out: Fail", "Console.Error: Fail");

            failByAssertion.testName.ShouldEqual(TestClass + ".FailByAssertion");
            failByAssertion.outcome.ShouldEqual("Failed");
            int.Parse(failByAssertion.durationMilliseconds).ShouldBeGreaterThanOrEqualTo(0);
            failByAssertion.ErrorMessage.Lines().ShouldEqual(
                "Assertion Failure",
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

        static AppVeyorListener.TestResult Deserialize(string content)
        {
            var deserializer = new DataContractJsonSerializer(typeof(AppVeyorListener.TestResult));

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(content)))
                return (AppVeyorListener.TestResult) deserializer.ReadObject(stream);
        }
    }
}