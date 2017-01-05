namespace Fixie.Tests.VisualStudio.TestAdapter
{
    using System;
    using System.Collections.Generic;
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
            var convention = SampleTestClassConvention.Build();
            var testClass = FullName<SampleTestClass>();

            using (var executionRecorder = new ExecutionRecorder(recorder, assemblyPath))
            using (var console = new RedirectedConsole())
            {
                var listener = new VisualStudioListener(executionRecorder);

                typeof(SampleTestClass).Run(listener, convention);

                console.Lines()
                    .ShouldEqual(
                        "Console.Out: Fail",
                        "Console.Error: Fail",
                        "Console.Out: FailByAssertion",
                        "Console.Error: FailByAssertion",
                        "Console.Out: Pass",
                        "Console.Error: Pass");
            }

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

            var skipWithReason = results[0];
            var skipWithoutReason = results[1];
            var fail = results[2];
            var failByAssertion = results[3];
            var pass = results[4];

            skipWithReason.TestCase.FullyQualifiedName.ShouldEqual(testClass + ".SkipWithReason");
            skipWithReason.TestCase.DisplayName.ShouldEqual(testClass + ".SkipWithReason");
            skipWithReason.TestCase.ExecutorUri.ToString().ShouldEqual("executor://fixie.visualstudio/");
            skipWithReason.Outcome.ShouldEqual(TestOutcome.Skipped);
            skipWithReason.ErrorMessage.ShouldEqual("Skipped with reason.");
            skipWithReason.ErrorStackTrace.ShouldBeNull();
            skipWithReason.DisplayName.ShouldEqual(testClass + ".SkipWithReason");
            skipWithReason.Messages.ShouldBeEmpty();
            skipWithReason.Duration.ShouldEqual(TimeSpan.Zero);

            skipWithoutReason.TestCase.FullyQualifiedName.ShouldEqual(testClass + ".SkipWithoutReason");
            skipWithoutReason.TestCase.DisplayName.ShouldEqual(testClass + ".SkipWithoutReason");
            skipWithoutReason.TestCase.ExecutorUri.ToString().ShouldEqual("executor://fixie.visualstudio/");
            skipWithoutReason.Outcome.ShouldEqual(TestOutcome.Skipped);
            skipWithoutReason.ErrorMessage.ShouldBeNull();
            skipWithoutReason.ErrorStackTrace.ShouldBeNull();
            skipWithoutReason.DisplayName.ShouldEqual(testClass + ".SkipWithoutReason");
            skipWithoutReason.Messages.ShouldBeEmpty();
            skipWithoutReason.Duration.ShouldEqual(TimeSpan.Zero);

            fail.TestCase.FullyQualifiedName.ShouldEqual(testClass + ".Fail");
            fail.TestCase.DisplayName.ShouldEqual(testClass + ".Fail");
            fail.TestCase.ExecutorUri.ToString().ShouldEqual("executor://fixie.visualstudio/");
            fail.Outcome.ShouldEqual(TestOutcome.Failed);
            fail.ErrorMessage.ShouldEqual("'Fail' failed!");
            fail.ErrorStackTrace
                .CleanStackTraceLineNumbers()
                .Lines()
                .ShouldEqual(
                    "Fixie.Tests.FailureException",
                    "'Fail' failed!",
                    At<SampleTestClass>("Fail()"));
            fail.DisplayName.ShouldEqual(testClass + ".Fail");
            fail.Messages.Count.ShouldEqual(1);
            fail.Messages[0].Category.ShouldEqual(TestResultMessage.StandardOutCategory);
            fail.Messages[0].Text.Lines().ShouldEqual("Console.Out: Fail", "Console.Error: Fail");
            fail.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);

            failByAssertion.TestCase.FullyQualifiedName.ShouldEqual(testClass + ".FailByAssertion");
            failByAssertion.TestCase.DisplayName.ShouldEqual(testClass + ".FailByAssertion");
            failByAssertion.TestCase.ExecutorUri.ToString().ShouldEqual("executor://fixie.visualstudio/");
            failByAssertion.Outcome.ShouldEqual(TestOutcome.Failed);
            failByAssertion.ErrorMessage.Lines().ShouldEqual("Assert.Equal() Failure",
                "Expected: 2",
                "Actual:   1");
            failByAssertion.ErrorStackTrace
                .CleanStackTraceLineNumbers()
                .Lines()
                .ShouldEqual(
                    "Assert.Equal() Failure",
                    "Expected: 2",
                    "Actual:   1",
                    At<SampleTestClass>("FailByAssertion()"));
            failByAssertion.DisplayName.ShouldEqual(testClass + ".FailByAssertion");
            failByAssertion.Messages.Count.ShouldEqual(1);
            failByAssertion.Messages[0].Category.ShouldEqual(TestResultMessage.StandardOutCategory);
            failByAssertion.Messages[0].Text.Lines().ShouldEqual("Console.Out: FailByAssertion", "Console.Error: FailByAssertion");
            failByAssertion.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);

            pass.TestCase.FullyQualifiedName.ShouldEqual(testClass + ".Pass");
            pass.TestCase.DisplayName.ShouldEqual(testClass + ".Pass");
            pass.TestCase.ExecutorUri.ToString().ShouldEqual("executor://fixie.visualstudio/");
            pass.Outcome.ShouldEqual(TestOutcome.Passed);
            pass.ErrorMessage.ShouldBeNull();
            pass.ErrorStackTrace.ShouldBeNull();
            pass.DisplayName.ShouldEqual(testClass + ".Pass");
            pass.Messages.Count.ShouldEqual(1);
            pass.Messages[0].Category.ShouldEqual(TestResultMessage.StandardOutCategory);
            pass.Messages[0].Text.Lines().ShouldEqual("Console.Out: Pass", "Console.Error: Pass");
            pass.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
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
    }
}