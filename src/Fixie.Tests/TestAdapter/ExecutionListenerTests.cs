namespace Fixie.Tests.TestAdapter
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fixie.TestAdapter;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
    using Assertions;
    using Reports;
    using static System.Environment;

    public class ExecutionListenerTests : MessagingTests
    {
        public async Task ShouldMapMessagesToVsTestExecutionRecorder()
        {
            const string assemblyPath = "assembly.path.dll";
            var recorder = new StubExecutionRecorder();

            var listener = new ExecutionListener(recorder, assemblyPath);

            var output = await RunAsync(listener);

            output.Console
                .ShouldBe(
                    "Console.Out: Fail",
                    "Console.Error: Fail",
                    "Console.Out: FailByAssertion",
                    "Console.Error: FailByAssertion",
                    "Console.Out: Pass",
                    "Console.Error: Pass");

            var messages = recorder.Messages;

            messages.Count.ShouldBe(12);

            foreach (var message in messages)
            {
                if (message is TestResult result)
                {
                    result.Traits.ShouldBeEmpty();
                    result.Attachments.ShouldBeEmpty();
                    result.ComputerName.ShouldBe(MachineName);
                }
            }

            var failStart = (TestCase)messages[0];
            var fail = (TestResult)messages[1];
            
            var failByAssertionStart = (TestCase)messages[2];
            var failByAssertion = (TestResult)messages[3];
            
            var passStart = (TestCase)messages[4];
            var pass = (TestResult)messages[5];
            
            var skipWithReason = (TestResult)messages[6];
            
            var skipWithoutReason = (TestResult)messages[7];
            
            var shouldBeStringPassStart = (TestCase)messages[8];
            var shouldBeStringPass = (TestResult)messages[9];
            var shouldBeStringFailStart = (TestCase)messages[10];
            var shouldBeStringFail = (TestResult)messages[11];

            failStart.ShouldBeExecutionTimeTest(TestClass + ".Fail", assemblyPath);

            fail.TestCase.ShouldBeExecutionTimeTest(TestClass+".Fail", assemblyPath);
            fail.TestCase.DisplayName.ShouldBe(TestClass+".Fail");
            fail.Outcome.ShouldBe(TestOutcome.Failed);
            fail.ErrorMessage.ShouldBe("'Fail' failed!");
            fail.ErrorStackTrace
                .Lines()
                .CleanStackTraceLineNumbers()
                .ShouldBe("Fixie.Tests.FailureException", At("Fail()"));
            fail.DisplayName.ShouldBe(TestClass+".Fail");
            fail.Messages.Count.ShouldBe(1);
            fail.Messages[0].Category.ShouldBe(TestResultMessage.StandardOutCategory);
            fail.Messages[0].Text.Lines().ShouldBe("Console.Out: Fail", "Console.Error: Fail");
            fail.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);

            failByAssertionStart.ShouldBeExecutionTimeTest(TestClass + ".FailByAssertion", assemblyPath);

            failByAssertion.TestCase.ShouldBeExecutionTimeTest(TestClass+".FailByAssertion", assemblyPath);
            failByAssertion.TestCase.DisplayName.ShouldBe(TestClass+".FailByAssertion");
            failByAssertion.Outcome.ShouldBe(TestOutcome.Failed);
            failByAssertion.ErrorMessage.Lines().ShouldBe(
                "Expected: 2",
                "Actual:   1");
            failByAssertion.ErrorStackTrace
                .Lines()
                .CleanStackTraceLineNumbers()
                .ShouldBe("Fixie.Tests.Assertions.AssertException", At("FailByAssertion()"));
            failByAssertion.DisplayName.ShouldBe(TestClass+".FailByAssertion");
            failByAssertion.Messages.Count.ShouldBe(1);
            failByAssertion.Messages[0].Category.ShouldBe(TestResultMessage.StandardOutCategory);
            failByAssertion.Messages[0].Text.Lines().ShouldBe("Console.Out: FailByAssertion", "Console.Error: FailByAssertion");
            failByAssertion.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);

            passStart.ShouldBeExecutionTimeTest(TestClass + ".Pass", assemblyPath);

            pass.TestCase.ShouldBeExecutionTimeTest(TestClass+".Pass", assemblyPath);
            pass.TestCase.DisplayName.ShouldBe(TestClass+".Pass");
            pass.Outcome.ShouldBe(TestOutcome.Passed);
            pass.ErrorMessage.ShouldBe(null);
            pass.ErrorStackTrace.ShouldBe(null);
            pass.DisplayName.ShouldBe(TestClass+".Pass");
            pass.Messages.Count.ShouldBe(1);
            pass.Messages[0].Category.ShouldBe(TestResultMessage.StandardOutCategory);
            pass.Messages[0].Text.Lines().ShouldBe("Console.Out: Pass", "Console.Error: Pass");
            pass.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);

            skipWithReason.TestCase.ShouldBeExecutionTimeTest(TestClass+".SkipWithReason", assemblyPath);
            skipWithReason.TestCase.DisplayName.ShouldBe(TestClass+".SkipWithReason");
            skipWithReason.Outcome.ShouldBe(TestOutcome.Skipped);
            skipWithReason.ErrorMessage.ShouldBe("⚠ Skipped with reason.");
            skipWithReason.ErrorStackTrace.ShouldBe(null);
            skipWithReason.DisplayName.ShouldBe(TestClass+".SkipWithReason");
            skipWithReason.Messages.ShouldBeEmpty();
            skipWithReason.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);

            skipWithoutReason.TestCase.ShouldBeExecutionTimeTest(TestClass+".SkipWithoutReason", assemblyPath);
            skipWithoutReason.TestCase.DisplayName.ShouldBe(TestClass+".SkipWithoutReason");
            skipWithoutReason.Outcome.ShouldBe(TestOutcome.Skipped);
            skipWithoutReason.ErrorMessage.ShouldBe(null);
            skipWithoutReason.ErrorStackTrace.ShouldBe(null);
            skipWithoutReason.DisplayName.ShouldBe(TestClass+".SkipWithoutReason");
            skipWithoutReason.Messages.ShouldBeEmpty();
            skipWithoutReason.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);

            shouldBeStringPassStart.ShouldBeExecutionTimeTest(GenericTestClass + ".ShouldBeString", assemblyPath);

            shouldBeStringPass.TestCase.ShouldBeExecutionTimeTest(GenericTestClass+".ShouldBeString", assemblyPath);
            shouldBeStringPass.TestCase.DisplayName.ShouldBe(GenericTestClass+".ShouldBeString");
            shouldBeStringPass.Outcome.ShouldBe(TestOutcome.Passed);
            shouldBeStringPass.ErrorMessage.ShouldBe(null);
            shouldBeStringPass.ErrorStackTrace.ShouldBe(null);
            shouldBeStringPass.DisplayName.ShouldBe(GenericTestClass+".ShouldBeString<System.String>(\"abc\")");
            shouldBeStringPass.Messages.ShouldBeEmpty();
            shouldBeStringPass.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);

            shouldBeStringFailStart.ShouldBeExecutionTimeTest(GenericTestClass + ".ShouldBeString", assemblyPath);

            shouldBeStringFail.TestCase.ShouldBeExecutionTimeTest(GenericTestClass+".ShouldBeString", assemblyPath);
            shouldBeStringFail.TestCase.DisplayName.ShouldBe(GenericTestClass+".ShouldBeString");
            shouldBeStringFail.Outcome.ShouldBe(TestOutcome.Failed);
            shouldBeStringFail.ErrorMessage.Lines().ShouldBe(
                "Expected: System.String",
                "Actual:   System.Int32");
            shouldBeStringFail.ErrorStackTrace
                .Lines()
                .CleanStackTraceLineNumbers()
                .ShouldBe(
                    "Fixie.Tests.Assertions.AssertException",
                    At<SampleGenericTestClass>("ShouldBeString[T](T genericArgument)"));
            shouldBeStringFail.DisplayName.ShouldBe(GenericTestClass+".ShouldBeString<System.Int32>(123)");
            shouldBeStringFail.Messages.ShouldBeEmpty();
            shouldBeStringFail.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
        }

        class StubExecutionRecorder : ITestExecutionRecorder
        {
            public List<object> Messages { get; } = new List<object>();

            public void RecordStart(TestCase testCase)
                => Messages.Add(testCase);

            public void RecordResult(TestResult testResult)
                => Messages.Add(testResult);

            public void SendMessage(TestMessageLevel testMessageLevel, string message)
                => throw new NotImplementedException();

            public void RecordEnd(TestCase testCase, TestOutcome outcome)
                => throw new NotImplementedException();

            public void RecordAttachments(IList<AttachmentSet> attachmentSets)
                => throw new NotImplementedException();
        }
    }
}