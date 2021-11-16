namespace Fixie.Tests.Reports
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Assertions;
    using Fixie.Reports;
    using static Utility;

    public class AppVeyorReportTests : MessagingTests
    {
        public async Task ShouldReportResultsToAppVeyorBuildWorkerApi()
        {
            var results = new List<AppVeyorReport.Result>();

            var report = new AppVeyorReport(GetTestEnvironment(), "http://localhost:4567", (uri, content) =>
            {
                uri.ShouldBe("http://localhost:4567/api/tests");
                results.Add(content);
                return Task.CompletedTask;
            });

            var output = await Run(report);

            output.Console
                .ShouldBe(
                    "Standard Out: Fail",
                    "Standard Out: FailByAssertion",
                    "Standard Out: Pass");

            results.Count.ShouldBe(7);

            foreach (var result in results)
            {
                result.TestFramework.ShouldBe("Fixie");
                result.FileName.ShouldBe($"Fixie.Tests (.NETCoreApp,Version=v{Environment.Version.ToString(2)})");
            }

            var fail = results[0];
            var failByAssertion = results[1];
            var pass = results[2];
            var skip = results[3];
            var shouldBeStringPassA = results[4];
            var shouldBeStringPassB = results[5];
            var shouldBeStringFail = results[6];

            fail.TestName.ShouldBe(TestClass + ".Fail");
            fail.Outcome.ShouldBe("Failed");
            int.Parse(fail.DurationMilliseconds).ShouldBeGreaterThanOrEqualTo(0);
            fail.ErrorMessage.ShouldBe("'Fail' failed!");
            fail.ErrorStackTrace
                .Lines()
                .NormalizeStackTraceLines()
                .ShouldBe("Fixie.Tests.FailureException", At("Fail()"));
            fail.StdOut.Lines().ShouldBe("Standard Out: Fail");

            failByAssertion.TestName.ShouldBe(TestClass + ".FailByAssertion");
            failByAssertion.Outcome.ShouldBe("Failed");
            int.Parse(failByAssertion.DurationMilliseconds).ShouldBeGreaterThanOrEqualTo(0);
            failByAssertion.ErrorMessage.Lines().ShouldBe(
                "Expected: 2",
                "Actual:   1");
            failByAssertion.ErrorStackTrace
                .Lines()
                .NormalizeStackTraceLines()
                .ShouldBe("Fixie.Tests.Assertions.AssertException", At("FailByAssertion()"));
            failByAssertion.StdOut.Lines().ShouldBe("Standard Out: FailByAssertion");

            pass.TestName.ShouldBe(TestClass + ".Pass");
            pass.Outcome.ShouldBe("Passed");
            int.Parse(pass.DurationMilliseconds).ShouldBeGreaterThanOrEqualTo(0);
            pass.ErrorMessage.ShouldBe(null);
            pass.ErrorStackTrace.ShouldBe(null);
            pass.StdOut.Lines().ShouldBe("Standard Out: Pass");

            skip.TestName.ShouldBe(TestClass + ".Skip");
            skip.Outcome.ShouldBe("Skipped");
            int.Parse(skip.DurationMilliseconds).ShouldBeGreaterThanOrEqualTo(0);
            skip.ErrorMessage.ShouldBe("⚠ Skipped with attribute.");
            skip.ErrorStackTrace.ShouldBe(null);
            skip.StdOut.ShouldBe("");

            shouldBeStringPassA.TestName.ShouldBe(GenericTestClass + ".ShouldBeString<System.String>(\"A\")");
            shouldBeStringPassA.Outcome.ShouldBe("Passed");
            int.Parse(shouldBeStringPassA.DurationMilliseconds).ShouldBeGreaterThanOrEqualTo(0);
            shouldBeStringPassA.ErrorMessage.ShouldBe(null);
            shouldBeStringPassA.ErrorStackTrace.ShouldBe(null);
            shouldBeStringPassA.StdOut.ShouldBe("");

            shouldBeStringPassB.TestName.ShouldBe(GenericTestClass + ".ShouldBeString<System.String>(\"B\")");
            shouldBeStringPassB.Outcome.ShouldBe("Passed");
            int.Parse(shouldBeStringPassB.DurationMilliseconds).ShouldBeGreaterThanOrEqualTo(0);
            shouldBeStringPassB.ErrorMessage.ShouldBe(null);
            shouldBeStringPassB.ErrorStackTrace.ShouldBe(null);
            shouldBeStringPassB.StdOut.ShouldBe("");

            shouldBeStringFail.TestName.ShouldBe(GenericTestClass + ".ShouldBeString<System.Int32>(123)");
            shouldBeStringFail.Outcome.ShouldBe("Failed");
            int.Parse(shouldBeStringFail.DurationMilliseconds).ShouldBeGreaterThanOrEqualTo(0);
            shouldBeStringFail.ErrorMessage.Lines().ShouldBe(
                "Expected: System.String",
                "Actual:   System.Int32");
            shouldBeStringFail.ErrorStackTrace
                .Lines()
                .NormalizeStackTraceLines()
                .ShouldBe("Fixie.Tests.Assertions.AssertException", At<SampleGenericTestClass>("ShouldBeString[T](T genericArgument)"));
            shouldBeStringFail.StdOut.ShouldBe("");
        }
    }
}