namespace Fixie.Tests.Reports
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Assertions;
    using Fixie.Reports;

    public class AppVeyorReportTests : MessagingTests
    {
        public async Task ShouldReportResultsToAppVeyorBuildWorkerApi()
        {
            var results = new List<AppVeyorReport.TestResult>();

            var report = new AppVeyorReport("http://localhost:4567", (uri, content) =>
            {
                uri.ShouldBe("http://localhost:4567/api/tests");
                results.Add(content);
                return Task.CompletedTask;
            });

            var output = await RunAsync(report);

            output.Console
                .ShouldBe(
                    "Console.Out: Fail",
                    "Console.Error: Fail",
                    "Console.Out: FailByAssertion",
                    "Console.Error: FailByAssertion",
                    "Console.Out: Pass",
                    "Console.Error: Pass");

            results.Count.ShouldBe(7);

            foreach (var result in results)
            {
                result.TestFramework.ShouldBe("Fixie");
                result.FileName.ShouldBe("Fixie.Tests (.NETCoreApp,Version=v3.1)");
            }

            var fail = results[0];
            var failByAssertion = results[1];
            var pass = results[2];
            var skipWithReason = results[3];
            var skipWithoutReason = results[4];
            var shouldBeStringPass = results[5];
            var shouldBeStringFail = results[6];

            fail.TestName.ShouldBe(TestClass + ".Fail");
            fail.Outcome.ShouldBe("Failed");
            int.Parse(fail.DurationMilliseconds).ShouldBeGreaterThanOrEqualTo(0);
            fail.ErrorMessage.ShouldBe("'Fail' failed!");
            fail.ErrorStackTrace
                .Lines()
                .CleanStackTraceLineNumbers()
                .ShouldBe("Fixie.Tests.FailureException", At("Fail()"));
            fail.StdOut.Lines().ShouldBe("Console.Out: Fail", "Console.Error: Fail");

            failByAssertion.TestName.ShouldBe(TestClass + ".FailByAssertion");
            failByAssertion.Outcome.ShouldBe("Failed");
            int.Parse(failByAssertion.DurationMilliseconds).ShouldBeGreaterThanOrEqualTo(0);
            failByAssertion.ErrorMessage.Lines().ShouldBe(
                "Expected: 2",
                "Actual:   1");
            failByAssertion.ErrorStackTrace
                .Lines()
                .CleanStackTraceLineNumbers()
                .ShouldBe("Fixie.Tests.Assertions.AssertException", At("FailByAssertion()"));
            failByAssertion.StdOut.Lines().ShouldBe("Console.Out: FailByAssertion", "Console.Error: FailByAssertion");

            pass.TestName.ShouldBe(TestClass + ".Pass");
            pass.Outcome.ShouldBe("Passed");
            int.Parse(pass.DurationMilliseconds).ShouldBeGreaterThanOrEqualTo(0);
            pass.ErrorMessage.ShouldBe(null);
            pass.ErrorStackTrace.ShouldBe(null);
            pass.StdOut.Lines().ShouldBe("Console.Out: Pass", "Console.Error: Pass");

            skipWithReason.TestName.ShouldBe(TestClass + ".SkipWithReason");
            skipWithReason.Outcome.ShouldBe("Skipped");
            int.Parse(skipWithReason.DurationMilliseconds).ShouldBeGreaterThanOrEqualTo(0);
            skipWithReason.ErrorMessage.ShouldBe("⚠ Skipped with reason.");
            skipWithReason.ErrorStackTrace.ShouldBe(null);
            skipWithReason.StdOut.ShouldBe("");

            skipWithoutReason.TestName.ShouldBe(TestClass + ".SkipWithoutReason");
            skipWithoutReason.Outcome.ShouldBe("Skipped");
            int.Parse(skipWithoutReason.DurationMilliseconds).ShouldBeGreaterThanOrEqualTo(0);
            skipWithoutReason.ErrorMessage.ShouldBe(null);
            skipWithoutReason.ErrorStackTrace.ShouldBe(null);
            skipWithoutReason.StdOut.ShouldBe("");

            shouldBeStringPass.TestName.ShouldBe(GenericTestClass + ".ShouldBeString<System.String>(\"abc\")");
            shouldBeStringPass.Outcome.ShouldBe("Passed");
            int.Parse(shouldBeStringPass.DurationMilliseconds).ShouldBeGreaterThanOrEqualTo(0);
            shouldBeStringPass.ErrorMessage.ShouldBe(null);
            shouldBeStringPass.ErrorStackTrace.ShouldBe(null);
            shouldBeStringPass.StdOut.ShouldBe("");

            shouldBeStringFail.TestName.ShouldBe(GenericTestClass + ".ShouldBeString<System.Int32>(123)");
            shouldBeStringFail.Outcome.ShouldBe("Failed");
            int.Parse(shouldBeStringFail.DurationMilliseconds).ShouldBeGreaterThanOrEqualTo(0);
            shouldBeStringFail.ErrorMessage.Lines().ShouldBe(
                "Expected: System.String",
                "Actual:   System.Int32");
            shouldBeStringFail.ErrorStackTrace
                .Lines()
                .CleanStackTraceLineNumbers()
                .ShouldBe("Fixie.Tests.Assertions.AssertException", At<SampleGenericTestClass>("ShouldBeString[T](T genericArgument)"));
            shouldBeStringFail.StdOut.ShouldBe("");
        }
    }
}