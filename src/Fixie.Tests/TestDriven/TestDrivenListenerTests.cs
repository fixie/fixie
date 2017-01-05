using TestDriven.Framework;

namespace Fixie.Tests.TestDriven
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Fixie.Internal;
    using Fixie.TestDriven;
    using Should;
    using static Utility;

    public class TestDrivenListenerTests
    {
        public void ShouldReportResultsToTestDrivenDotNet()
        {
            var testDriven = new StubTestListener();

            using (var console = new RedirectedConsole())
            {
                var listener = new TestDrivenListener(testDriven);

                var convention = SampleTestClassConvention.Build();

                typeof(SampleTestClass).Run(listener, convention);

                var summary = listener.Summary;
                summary.Passed.ShouldEqual(1);
                summary.Failed.ShouldEqual(2);
                summary.Skipped.ShouldEqual(2);
                summary.Total.ShouldEqual(5);

                var testClass = FullName<SampleTestClass>();

                console.Lines()
                    .ShouldEqual(
                        "Console.Out: Fail",
                        "Console.Error: Fail",
                        "Console.Out: FailByAssertion",
                        "Console.Error: FailByAssertion",
                        "Console.Out: Pass",
                        "Console.Error: Pass");

                var results = testDriven.TestResults;
                results.Count.ShouldEqual(5);

                foreach (var result in results)
                {
                    result.FixtureType.ShouldEqual(null);
                    result.Method.ShouldEqual(null);
                    result.TimeSpan.ShouldEqual(TimeSpan.Zero);
                    result.TotalTests.ShouldEqual(0);
                    result.TestRunnerName.ShouldBeNull();
                }

                results[0].Name.ShouldEqual(testClass + ".SkipWithReason");
                results[0].State.ShouldEqual(TestState.Ignored);
                results[0].Message.ShouldEqual("Skipped with reason.");
                results[0].StackTrace.ShouldBeNull();

                results[1].Name.ShouldEqual(testClass + ".SkipWithoutReason");
                results[1].State.ShouldEqual(TestState.Ignored);
                results[1].Message.ShouldBeNull();
                results[1].StackTrace.ShouldBeNull();

                results[2].Name.ShouldEqual(testClass + ".Fail");
                results[2].State.ShouldEqual(TestState.Failed);
                results[2].Message.ShouldEqual("Fixie.Tests.FailureException");
                results[2].StackTrace.Lines().Select(CleanBrittleValues).ShouldEqual(
                    "'Fail' failed!",
                    At<SampleTestClass>("Fail()"));

                results[3].Name.ShouldEqual(testClass + ".FailByAssertion");
                results[3].State.ShouldEqual(TestState.Failed);
                results[3].Message.ShouldEqual("");
                results[3].StackTrace.Lines().Select(CleanBrittleValues).ShouldEqual(
                    "Assert.Equal() Failure",
                    "Expected: 2",
                    "Actual:   1",
                    At<SampleTestClass>("FailByAssertion()"));

                results[4].Name.ShouldEqual(testClass + ".Pass");
                results[4].State.ShouldEqual(TestState.Passed);
                results[4].Message.ShouldBeNull();
                results[4].StackTrace.ShouldBeNull();
            }
        }

        static string CleanBrittleValues(string actualRawContent)
        {
            //Avoid brittle assertion introduced by stack trace line numbers.
            var cleaned = Regex.Replace(actualRawContent, @":line \d+", ":line #");

            return cleaned;
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
