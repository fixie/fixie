namespace Fixie.Tests.VisualStudio.TestAdapter
{
    using System;
    using System.Collections.Generic;
    using Fixie.Internal;
    using Fixie.VisualStudio.TestAdapter;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
    using Should;

    public class VisualStudioExecutionListenerTests : MessagingTests
    {
        public void ShouldReportResultsToExecutionRecorder()
        {
            const string assemblyPath = "assembly.path.dll";
            var recorder = new StubExecutionRecorder();
            var listener = new VisualStudioExecutionListener(recorder, assemblyPath);

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

            skipWithReason.TestCase.FullyQualifiedName.ShouldEqual(TestClass + ".SkipWithReason");
            skipWithReason.TestCase.DisplayName.ShouldEqual(TestClass + ".SkipWithReason");
            skipWithReason.TestCase.ExecutorUri.ToString().ShouldEqual("executor://fixie.visualstudio/");
            skipWithReason.Outcome.ShouldEqual(TestOutcome.Skipped);
            skipWithReason.ErrorMessage.ShouldEqual("Skipped with reason.");
            skipWithReason.ErrorStackTrace.ShouldBeNull();
            skipWithReason.DisplayName.ShouldEqual(TestClass + ".SkipWithReason");
            skipWithReason.Messages.ShouldBeEmpty();
            skipWithReason.Duration.ShouldEqual(TimeSpan.Zero);

            skipWithoutReason.TestCase.FullyQualifiedName.ShouldEqual(TestClass + ".SkipWithoutReason");
            skipWithoutReason.TestCase.DisplayName.ShouldEqual(TestClass + ".SkipWithoutReason");
            skipWithoutReason.TestCase.ExecutorUri.ToString().ShouldEqual("executor://fixie.visualstudio/");
            skipWithoutReason.Outcome.ShouldEqual(TestOutcome.Skipped);
            skipWithoutReason.ErrorMessage.ShouldBeNull();
            skipWithoutReason.ErrorStackTrace.ShouldBeNull();
            skipWithoutReason.DisplayName.ShouldEqual(TestClass + ".SkipWithoutReason");
            skipWithoutReason.Messages.ShouldBeEmpty();
            skipWithoutReason.Duration.ShouldEqual(TimeSpan.Zero);

            fail.TestCase.FullyQualifiedName.ShouldEqual(TestClass + ".Fail");
            fail.TestCase.DisplayName.ShouldEqual(TestClass + ".Fail");
            fail.TestCase.ExecutorUri.ToString().ShouldEqual("executor://fixie.visualstudio/");
            fail.Outcome.ShouldEqual(TestOutcome.Failed);
            fail.ErrorMessage.ShouldEqual("'Fail' failed!");
            fail.ErrorStackTrace
                .CleanStackTraceLineNumbers()
                .Lines()
                .ShouldEqual("Fixie.Tests.FailureException", At("Fail()"));
            fail.DisplayName.ShouldEqual(TestClass + ".Fail");
            fail.Messages.Count.ShouldEqual(1);
            fail.Messages[0].Category.ShouldEqual(TestResultMessage.StandardOutCategory);
            fail.Messages[0].Text.Lines().ShouldEqual("Console.Out: Fail", "Console.Error: Fail");
            fail.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);

            failByAssertion.TestCase.FullyQualifiedName.ShouldEqual(TestClass + ".FailByAssertion");
            failByAssertion.TestCase.DisplayName.ShouldEqual(TestClass + ".FailByAssertion");
            failByAssertion.TestCase.ExecutorUri.ToString().ShouldEqual("executor://fixie.visualstudio/");
            failByAssertion.Outcome.ShouldEqual(TestOutcome.Failed);
            failByAssertion.ErrorMessage.Lines().ShouldEqual(
                "Assert.Equal() Failure",
                "Expected: 2",
                "Actual:   1");
            failByAssertion.ErrorStackTrace
                .CleanStackTraceLineNumbers()
                .ShouldEqual(At("FailByAssertion()"));
            failByAssertion.DisplayName.ShouldEqual(TestClass + ".FailByAssertion");
            failByAssertion.Messages.Count.ShouldEqual(1);
            failByAssertion.Messages[0].Category.ShouldEqual(TestResultMessage.StandardOutCategory);
            failByAssertion.Messages[0].Text.Lines().ShouldEqual("Console.Out: FailByAssertion", "Console.Error: FailByAssertion");
            failByAssertion.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);

            pass.TestCase.FullyQualifiedName.ShouldEqual(TestClass + ".Pass");
            pass.TestCase.DisplayName.ShouldEqual(TestClass + ".Pass");
            pass.TestCase.ExecutorUri.ToString().ShouldEqual("executor://fixie.visualstudio/");
            pass.Outcome.ShouldEqual(TestOutcome.Passed);
            pass.ErrorMessage.ShouldBeNull();
            pass.ErrorStackTrace.ShouldBeNull();
            pass.DisplayName.ShouldEqual(TestClass + ".Pass");
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