using TestDriven.Framework;

namespace Fixie.Tests.TestDriven
{
    using System;
    using System.Collections.Generic;
    using Assertions;
    using Fixie.Internal;
    using Fixie.TestDriven;

    public class TestDrivenListenerTests : MessagingTests
    {
        public void ShouldReportResultsToTestDrivenDotNet()
        {
            var testDriven = new StubTestListener();
            var listener = new TestDrivenListener(testDriven);

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

            var results = testDriven.TestResults;
            results.Count.ShouldBe(5);

            foreach (var result in results)
            {
                result.FixtureType.ShouldBe(null);
                result.Method.ShouldBe(null);
                result.TimeSpan.ShouldBe(TimeSpan.Zero);
                result.TotalTests.ShouldBe(0);
                result.TestRunnerName.ShouldBe(null);
            }

            var fail = results[0];
            var failByAssertion = results[1];
            var pass = results[2];
            var skipWithReason = results[3];
            var skipWithoutReason = results[4];

            skipWithReason.Name.ShouldBe(TestClass + ".SkipWithReason");
            skipWithReason.State.ShouldBe(TestState.Ignored);
            skipWithReason.Message.ShouldBe("⚠ Skipped with reason.");
            skipWithReason.StackTrace.ShouldBe(null);

            skipWithoutReason.Name.ShouldBe(TestClass + ".SkipWithoutReason");
            skipWithoutReason.State.ShouldBe(TestState.Ignored);
            skipWithoutReason.Message.ShouldBe(null);
            skipWithoutReason.StackTrace.ShouldBe(null);

            fail.Name.ShouldBe(TestClass + ".Fail");
            fail.State.ShouldBe(TestState.Failed);
            fail.Message.ShouldBe("Fixie.Tests.FailureException");
            fail.StackTrace
                .CleanStackTraceLineNumbers()
                .Lines()
                .ShouldBe(
                    "'Fail' failed!",
                    "",
                    At("Fail()"));

            failByAssertion.Name.ShouldBe(TestClass + ".FailByAssertion");
            failByAssertion.State.ShouldBe(TestState.Failed);
            failByAssertion.Message.ShouldBe("Fixie.Assertions.ExpectedException");
            failByAssertion.StackTrace
                .CleanStackTraceLineNumbers()
                .Lines()
                .ShouldBe(
                    "Expected: 2",
                    "Actual:   1",
                    "",
                    At("FailByAssertion()"));

            pass.Name.ShouldBe(TestClass + ".Pass");
            pass.State.ShouldBe(TestState.Passed);
            pass.Message.ShouldBe(null);
            pass.StackTrace.ShouldBe(null);
        }

        class StubTestListener : ITestListener
        {
            public List<TestResult> TestResults { get; } = new List<TestResult>();

            public void TestFinished(TestResult summary)
            {
                TestResults.Add(summary);
            }

            public void WriteLine(string text, Category category)
            {
                throw new NotImplementedException();
            }

            public void TestResultsUrl(string resultsUrl)
            {
                throw new NotImplementedException();
            }
        }
    }
}