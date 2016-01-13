using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Fixie.Internal;
using Fixie.TestDriven;
using Should;
using TestDriven.Framework;

namespace Fixie.Tests.TestDriven
{
    public class TestDrivenListenerTests
    {
        public void ShouldReportResultsToTestDrivenDotNet()
        {
            var testDriven = new StubTestListener();

            using (var console = new RedirectedConsole())
            {
                var listener = new TestDrivenListener(testDriven);

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

                var results = testDriven.TestResults;
                results.Count.ShouldEqual(4);

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
                    "   at Fixie.Tests.TestDriven.TestDrivenListenerTests.PassFailTestClass.Fail() in " + PathToThisFile() + ":line #");
                
                results[3].Name.ShouldEqual(testClass + ".Pass(123)");
                results[3].State.ShouldEqual(TestState.Passed);
                results[3].Message.ShouldBeNull();
                results[3].StackTrace.ShouldBeNull();
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
    }
}
