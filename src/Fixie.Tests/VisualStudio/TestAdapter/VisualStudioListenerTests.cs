namespace Fixie.Tests.VisualStudio.TestAdapter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;
    using Execution;
    using Fixie.Internal;
    using Fixie.VisualStudio.TestAdapter;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
    using Should;
    using static Utility;

    public class VisualStudioListenerTests
    {
        public void ShouldAllowRunnersInOtherAppDomainsToReportTestDiscoveryAndExecutionToVisualStudio()
        {
            typeof(ExecutionRecorder).ShouldBeSafeAppDomainCommunicationInterface();
        }

        public void ShouldReportResultsToExecutionRecorder()
        {
            const string assemblyPath = "assembly.path.dll";
            var recorder = new StubExecutionRecorder();

            using (var executionRecorder = new ExecutionRecorder(recorder, assemblyPath))
            using (var console = new RedirectedConsole())
            {
                var listener = new VisualStudioListener(executionRecorder);

                var convention = SelfTestConvention.Build();
                convention.CaseExecution.Skip(x => x.Method.Has<SkipAttribute>(), x => x.Method.GetCustomAttribute<SkipAttribute>().Reason);

                typeof(SampleTestClass).Run(listener, convention);

                var testClass = FullName<SampleTestClass>();

                console.Lines()
                    .ShouldEqual(
                        "Console.Out: Fail",
                        "Console.Error: Fail",
                        "Console.Out: FailByAssertion",
                        "Console.Error: FailByAssertion",
                        "Console.Out: Pass",
                        "Console.Error: Pass");

                var results = recorder.TestResults;
                results.Count.ShouldEqual(5);

                foreach (var result in results)
                {
                    result.Traits.ShouldBeEmpty();
                    result.Attachments.ShouldBeEmpty();
                    result.ComputerName.ShouldEqual(Environment.MachineName);
                    result.TestCase.Traits.ShouldBeEmpty();
                    result.TestCase.LocalExtensionData.ShouldBeNull();
                    result.TestCase.Source.ShouldEqual("assembly.path.dll");

                    //Source locations are a discovery-time concern.
                    result.TestCase.CodeFilePath.ShouldBeNull();
                    result.TestCase.LineNumber.ShouldEqual(-1);
                }

                results[0].TestCase.FullyQualifiedName.ShouldEqual(testClass + ".SkipWithReason");
                results[0].TestCase.DisplayName.ShouldEqual(testClass + ".SkipWithReason");
                results[0].TestCase.ExecutorUri.ToString().ShouldEqual("executor://fixie.visualstudio/");
                results[0].Outcome.ShouldEqual(TestOutcome.Skipped);
                results[0].ErrorMessage.ShouldEqual("Skipped with reason.");
                results[0].ErrorStackTrace.ShouldBeNull();
                results[0].DisplayName.ShouldEqual(testClass + ".SkipWithReason");
                results[0].Messages.ShouldBeEmpty();
                results[0].Duration.ShouldEqual(TimeSpan.Zero);

                results[1].TestCase.FullyQualifiedName.ShouldEqual(testClass + ".SkipWithoutReason");
                results[1].TestCase.DisplayName.ShouldEqual(testClass + ".SkipWithoutReason");
                results[1].TestCase.ExecutorUri.ToString().ShouldEqual("executor://fixie.visualstudio/");
                results[1].Outcome.ShouldEqual(TestOutcome.Skipped);
                results[1].ErrorMessage.ShouldBeNull();
                results[1].ErrorStackTrace.ShouldBeNull();
                results[1].DisplayName.ShouldEqual(testClass + ".SkipWithoutReason");
                results[1].Messages.ShouldBeEmpty();
                results[1].Duration.ShouldEqual(TimeSpan.Zero);

                results[2].TestCase.FullyQualifiedName.ShouldEqual(testClass + ".Fail");
                results[2].TestCase.DisplayName.ShouldEqual(testClass + ".Fail");
                results[2].TestCase.ExecutorUri.ToString().ShouldEqual("executor://fixie.visualstudio/");
                results[2].Outcome.ShouldEqual(TestOutcome.Failed);
                results[2].ErrorMessage.ShouldEqual("'Fail' failed!");
                results[2].ErrorStackTrace.Lines().Select(CleanBrittleValues)
                    .ShouldEqual(
                        "Fixie.Tests.FailureException",
                        "'Fail' failed!",
                        At<SampleTestClass>("Fail()"));
                results[2].DisplayName.ShouldEqual(testClass + ".Fail");
                results[2].Messages.Count.ShouldEqual(1);
                results[2].Messages[0].Category.ShouldEqual(TestResultMessage.StandardOutCategory);
                results[2].Messages[0].Text.Lines().ShouldEqual("Console.Out: Fail", "Console.Error: Fail");
                results[2].Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);

                results[3].TestCase.FullyQualifiedName.ShouldEqual(testClass + ".FailByAssertion");
                results[3].TestCase.DisplayName.ShouldEqual(testClass + ".FailByAssertion");
                results[3].TestCase.ExecutorUri.ToString().ShouldEqual("executor://fixie.visualstudio/");
                results[3].Outcome.ShouldEqual(TestOutcome.Failed);
                results[3].ErrorMessage.Lines().ShouldEqual("Assert.Equal() Failure",
                    "Expected: 2",
                    "Actual:   1");
                results[3].ErrorStackTrace.Lines().Select(CleanBrittleValues)
                    .ShouldEqual(
                        "Should.Core.Exceptions.EqualException",
                        "Assert.Equal() Failure",
                        "Expected: 2",
                        "Actual:   1",
                        At<SampleTestClass>("FailByAssertion()"));
                results[3].DisplayName.ShouldEqual(testClass + ".FailByAssertion");
                results[3].Messages.Count.ShouldEqual(1);
                results[3].Messages[0].Category.ShouldEqual(TestResultMessage.StandardOutCategory);
                results[3].Messages[0].Text.Lines().ShouldEqual("Console.Out: FailByAssertion", "Console.Error: FailByAssertion");
                results[3].Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);

                results[4].TestCase.FullyQualifiedName.ShouldEqual(testClass + ".Pass");
                results[4].TestCase.DisplayName.ShouldEqual(testClass + ".Pass");
                results[4].TestCase.ExecutorUri.ToString().ShouldEqual("executor://fixie.visualstudio/");
                results[4].Outcome.ShouldEqual(TestOutcome.Passed);
                results[4].ErrorMessage.ShouldBeNull();
                results[4].ErrorStackTrace.ShouldBeNull();
                results[4].DisplayName.ShouldEqual(testClass + ".Pass");
                results[4].Messages.Count.ShouldEqual(1);
                results[4].Messages[0].Category.ShouldEqual(TestResultMessage.StandardOutCategory);
                results[4].Messages[0].Text.Lines().ShouldEqual("Console.Out: Pass", "Console.Error: Pass");
                results[4].Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
            }
        }

        static string CleanBrittleValues(string actualRawContent)
        {
            //Avoid brittle assertion introduced by stack trace line numbers.
            var cleaned = Regex.Replace(actualRawContent, @":line \d+", ":line #");

            return cleaned;
        }

        class StubExecutionRecorder : ITestExecutionRecorder
        {
            public List<TestResult> TestResults { get; } = new List<TestResult>();

            public void RecordResult(TestResult testResult)
                => TestResults.Add(testResult);

            public void SendMessage(TestMessageLevel testMessageLevel, string message)
                => NotImplemented();

            public void RecordStart(TestCase testCase)
                => NotImplemented();

            public void RecordEnd(TestCase testCase, TestOutcome outcome)
                => NotImplemented();

            public void RecordAttachments(IList<AttachmentSet> attachmentSets)
                => NotImplemented();

            static void NotImplemented()
            {
                throw new NotImplementedException();
            }
        }

        class SampleTestClass
        {
            public void Pass()
            {
                WhereAmI();
            }

            public void Fail()
            {
                WhereAmI();
                throw new FailureException();
            }

            public void FailByAssertion()
            {
                WhereAmI();
                1.ShouldEqual(2);
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