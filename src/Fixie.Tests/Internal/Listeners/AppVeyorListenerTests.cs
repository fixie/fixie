namespace Fixie.Tests.Internal.Listeners
{
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization.Json;
    using System.Text;
    using Assertions;
    using Fixie.Internal;
    using Fixie.Internal.Listeners;

    public class AppVeyorListenerTests : MessagingTests
    {
        public void ShouldReportResultsToAppVeyorBuildWorkerApi()
        {
            var results = new List<AppVeyorListener.TestResult>();

            var listener = new AppVeyorListener("http://localhost:4567", (uri, mediaType, content) =>
            {
                uri.ShouldBe("http://localhost:4567/api/tests");
                mediaType.ShouldBe("application/json");

                results.Add(Deserialize(content));
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

            results.Count.ShouldBe(5);

            foreach (var result in results)
            {
                result.TestFramework.ShouldBe("Fixie");

#if NET452
                result.FileName.ShouldBe("Fixie.Tests.exe");
#else
                result.FileName.ShouldBe("Fixie.Tests.dll");
#endif
            }

            var fail = results[0];
            var failByAssertion = results[1];
            var pass = results[2];
            var skipWithReason = results[3];
            var skipWithoutReason = results[4];

            skipWithReason.TestName.ShouldBe(TestClass + ".SkipWithReason");
            skipWithReason.Outcome.ShouldBe("Skipped");
            int.Parse(skipWithReason.DurationMilliseconds).ShouldBeGreaterThanOrEqualTo(0);
            skipWithReason.ErrorMessage.ShouldBe("⚠ Skipped with reason.");
            skipWithReason.ErrorStackTrace.ShouldBeNull();
            skipWithReason.StdOut.ShouldBeEmpty();

            skipWithoutReason.TestName.ShouldBe(TestClass + ".SkipWithoutReason");
            skipWithoutReason.Outcome.ShouldBe("Skipped");
            int.Parse(skipWithoutReason.DurationMilliseconds).ShouldBeGreaterThanOrEqualTo(0);
            skipWithoutReason.ErrorMessage.ShouldBeNull();
            skipWithoutReason.ErrorStackTrace.ShouldBeNull();
            skipWithoutReason.StdOut.ShouldBeEmpty();

            fail.TestName.ShouldBe(TestClass + ".Fail");
            fail.Outcome.ShouldBe("Failed");
            int.Parse(fail.DurationMilliseconds).ShouldBeGreaterThanOrEqualTo(0);
            fail.ErrorMessage.ShouldBe("'Fail' failed!");
            fail.ErrorStackTrace
                .CleanStackTraceLineNumbers()
                .Lines()
                .ShouldBe("Fixie.Tests.FailureException", At("Fail()"));
            fail.StdOut.Lines().ShouldBe("Console.Out: Fail", "Console.Error: Fail");

            failByAssertion.TestName.ShouldBe(TestClass + ".FailByAssertion");
            failByAssertion.Outcome.ShouldBe("Failed");
            int.Parse(failByAssertion.DurationMilliseconds).ShouldBeGreaterThanOrEqualTo(0);
            failByAssertion.ErrorMessage.Lines().ShouldBe(
                "Expected: 2",
                "Actual:   1");
            failByAssertion.ErrorStackTrace
                .CleanStackTraceLineNumbers()
                .Lines()
                .ShouldBe("Fixie.Assertions.ExpectedException", At("FailByAssertion()"));
            failByAssertion.StdOut.Lines().ShouldBe("Console.Out: FailByAssertion", "Console.Error: FailByAssertion");

            pass.TestName.ShouldBe(TestClass + ".Pass");
            pass.Outcome.ShouldBe("Passed");
            int.Parse(pass.DurationMilliseconds).ShouldBeGreaterThanOrEqualTo(0);
            pass.ErrorMessage.ShouldBeNull();
            pass.ErrorStackTrace.ShouldBeNull();
            pass.StdOut.Lines().ShouldBe("Console.Out: Pass", "Console.Error: Pass");
        }

        static AppVeyorListener.TestResult Deserialize(string content)
        {
            var deserializer = new DataContractJsonSerializer(typeof(AppVeyorListener.TestResult));

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(content)))
                return (AppVeyorListener.TestResult) deserializer.ReadObject(stream);
        }
    }
}