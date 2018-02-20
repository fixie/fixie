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
                result.TestFramework.ShouldEqual("Fixie");

#if NET471
                result.FileName.ShouldEqual("Fixie.Tests.exe");
#else
                result.FileName.ShouldEqual("Fixie.Tests.dll");
#endif
            }

            var fail = results[0];
            var failByAssertion = results[1];
            var pass = results[2];
            var skipWithReason = results[3];
            var skipWithoutReason = results[4];

            skipWithReason.TestName.ShouldEqual(TestClass + ".SkipWithReason");
            skipWithReason.Outcome.ShouldEqual("Skipped");
            skipWithReason.DurationMilliseconds.ShouldEqual("0");
            skipWithReason.ErrorMessage.ShouldEqual("Skipped with reason.");
            skipWithReason.ErrorStackTrace.ShouldBeNull();
            skipWithReason.StdOut.ShouldBeNull();

            skipWithoutReason.TestName.ShouldEqual(TestClass + ".SkipWithoutReason");
            skipWithoutReason.Outcome.ShouldEqual("Skipped");
            skipWithoutReason.DurationMilliseconds.ShouldEqual("0");
            skipWithoutReason.ErrorMessage.ShouldBeNull();
            skipWithoutReason.ErrorStackTrace.ShouldBeNull();
            skipWithoutReason.StdOut.ShouldBeNull();

            fail.TestName.ShouldEqual(TestClass + ".Fail");
            fail.Outcome.ShouldEqual("Failed");
            int.Parse(fail.DurationMilliseconds).ShouldBeGreaterThanOrEqualTo(0);
            fail.ErrorMessage.ShouldEqual("'Fail' failed!");
            fail.ErrorStackTrace
                .CleanStackTraceLineNumbers()
                .Lines()
                .ShouldEqual("Fixie.Tests.FailureException", At("Fail()"));
            fail.StdOut.Lines().ShouldEqual("Console.Out: Fail", "Console.Error: Fail");

            failByAssertion.TestName.ShouldEqual(TestClass + ".FailByAssertion");
            failByAssertion.Outcome.ShouldEqual("Failed");
            int.Parse(failByAssertion.DurationMilliseconds).ShouldBeGreaterThanOrEqualTo(0);
            failByAssertion.ErrorMessage.Lines().ShouldEqual(
                "Assertion Failure",
                "Expected: 2",
                "Actual:   1");
            failByAssertion.ErrorStackTrace
                .CleanStackTraceLineNumbers()
                .Lines()
                .ShouldEqual("Fixie.Assertions.AssertActualExpectedException", At("FailByAssertion()"));
            failByAssertion.StdOut.Lines().ShouldEqual("Console.Out: FailByAssertion", "Console.Error: FailByAssertion");

            pass.TestName.ShouldEqual(TestClass + ".Pass");
            pass.Outcome.ShouldEqual("Passed");
            int.Parse(pass.DurationMilliseconds).ShouldBeGreaterThanOrEqualTo(0);
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