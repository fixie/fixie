namespace Fixie.Tests.TestAdapter
{
    using System;
    using System.Collections.Generic;
    using Fixie.TestAdapter;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
    using Assertions;
    using static System.Environment;

    public class ExecutionListenerTests : MessagingTests
    {
        public void ShouldMapMessagesToVsTestExecutionRecorder()
        {
            const string assemblyPath = "assembly.path.dll";
            var recorder = new StubExecutionRecorder();

            var listener = new ExecutionListener(recorder, assemblyPath);

            Run(listener, out var console);

            console
                .ShouldBe(
                    "Console.Out: Fail",
                    "Console.Error: Fail",
                    "Console.Out: FailByAssertion",
                    "Console.Error: FailByAssertion",
                    "Console.Out: Pass",
                    "Console.Error: Pass");

            recorder.Messages.Count.ShouldBe(14);

            var starts = new List<TestCase>();
            var results = new List<TestResult>();

            bool expectStart = true;
            foreach (var message in recorder.Messages)
            {
                if (expectStart)
                    starts.Add((TestCase) message);
                else
                    results.Add((TestResult) message);

                expectStart = !expectStart;
            }

            starts.ShouldSatisfy(
                x => x.ShouldBeExecutionTimeTest(TestClass + ".Fail", assemblyPath),
                x => x.ShouldBeExecutionTimeTest(TestClass + ".FailByAssertion", assemblyPath),
                x => x.ShouldBeExecutionTimeTest(TestClass + ".Pass", assemblyPath),
                x => x.ShouldBeExecutionTimeTest(TestClass + ".SkipWithReason", assemblyPath),
                x => x.ShouldBeExecutionTimeTest(TestClass + ".SkipWithoutReason", assemblyPath),
                x => x.ShouldBeExecutionTimeTest(GenericTestClass + ".ShouldBeString", assemblyPath),
                x => x.ShouldBeExecutionTimeTest(GenericTestClass + ".ShouldBeString", assemblyPath)
            );

            results.Count.ShouldBe(7);

            foreach (var result in results)
            {
                result.Traits.ShouldBeEmpty();
                result.Attachments.ShouldBeEmpty();
                result.ComputerName.ShouldBe(MachineName);
            }

            var fail = results[0];
            var failByAssertion = results[1];
            var pass = results[2];
            var skipWithReason = results[3];
            var skipWithoutReason = results[4];
            var shouldBeStringPass = results[5];
            var shouldBeStringFail = results[6];

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

            shouldBeStringPass.TestCase.ShouldBeExecutionTimeTest(GenericTestClass+".ShouldBeString", assemblyPath);
            shouldBeStringPass.TestCase.DisplayName.ShouldBe(GenericTestClass+".ShouldBeString");
            shouldBeStringPass.Outcome.ShouldBe(TestOutcome.Passed);
            shouldBeStringPass.ErrorMessage.ShouldBe(null);
            shouldBeStringPass.ErrorStackTrace.ShouldBe(null);
            shouldBeStringPass.DisplayName.ShouldBe(GenericTestClass+".ShouldBeString<System.String>(\"abc\")");
            shouldBeStringPass.Messages.ShouldBeEmpty();
            shouldBeStringPass.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);

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