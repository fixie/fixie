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

    public class InProcessExecutionReportTests : MessagingTests
    {
        public async Task ShouldMapMessagesToVsTestExecutionRecorder()
        {
            const string assemblyPath = "assembly.path.dll";
            var recorder = new StubExecutionRecorder();

            var report = new InProcessExecutionReport(recorder, assemblyPath);

            var output = await Run(report);

            output.Console
                .ShouldBe(
                    "Standard Out: Fail",
                    "Standard Out: FailByAssertion",
                    "Standard Out: Pass");

            var messages = recorder.Messages;

            messages.Count.ShouldBe(13);

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
            
            var skip = (TestResult)messages[6];
            
            var shouldBeStringPassAStart = (TestCase)messages[7];
            var shouldBeStringPassA = (TestResult)messages[8];
            
            var shouldBeStringPassBStart = (TestCase)messages[9];
            var shouldBeStringPassB = (TestResult)messages[10];
            
            var shouldBeStringFailStart = (TestCase)messages[11];
            var shouldBeStringFail = (TestResult)messages[12];

            failStart.ShouldBeExecutionTimeTest(TestClass + ".Fail", assemblyPath);

            fail.TestCase.ShouldBeExecutionTimeTest(TestClass+".Fail", assemblyPath);
            fail.TestCase.DisplayName.ShouldBe(TestClass+".Fail");
            fail.Outcome.ShouldBe(TestOutcome.Failed);
            fail.ErrorMessage.ShouldBe("'Fail' failed!");
            fail.ErrorStackTrace
                .Lines()
                .NormalizeStackTraceLines()
                .ShouldBe("Fixie.Tests.FailureException", At("Fail()"));
            fail.DisplayName.ShouldBe(TestClass+".Fail");
            fail.Messages.Count.ShouldBe(1);
            fail.Messages[0].Category.ShouldBe(TestResultMessage.StandardOutCategory);
            fail.Messages[0].Text.Lines().ShouldBe("Standard Out: Fail");
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
                .NormalizeStackTraceLines()
                .ShouldBe("Fixie.Tests.Assertions.AssertException", At("FailByAssertion()"));
            failByAssertion.DisplayName.ShouldBe(TestClass+".FailByAssertion");
            failByAssertion.Messages.Count.ShouldBe(1);
            failByAssertion.Messages[0].Category.ShouldBe(TestResultMessage.StandardOutCategory);
            failByAssertion.Messages[0].Text.Lines().ShouldBe("Standard Out: FailByAssertion");
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
            pass.Messages[0].Text.Lines().ShouldBe("Standard Out: Pass");
            pass.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);

            skip.TestCase.ShouldBeExecutionTimeTest(TestClass+".Skip", assemblyPath);
            skip.TestCase.DisplayName.ShouldBe(TestClass+".Skip");
            skip.Outcome.ShouldBe(TestOutcome.Skipped);
            skip.ErrorMessage.ShouldBe("⚠ Skipped with attribute.");
            skip.ErrorStackTrace.ShouldBe(null);
            skip.DisplayName.ShouldBe(TestClass+".Skip");
            skip.Messages.ShouldBeEmpty();
            skip.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);

            shouldBeStringPassAStart.ShouldBeExecutionTimeTest(GenericTestClass + ".ShouldBeString", assemblyPath);

            shouldBeStringPassA.TestCase.ShouldBeExecutionTimeTest(GenericTestClass+".ShouldBeString", assemblyPath);
            shouldBeStringPassA.TestCase.DisplayName.ShouldBe(GenericTestClass+".ShouldBeString");
            shouldBeStringPassA.Outcome.ShouldBe(TestOutcome.Passed);
            shouldBeStringPassA.ErrorMessage.ShouldBe(null);
            shouldBeStringPassA.ErrorStackTrace.ShouldBe(null);
            shouldBeStringPassA.DisplayName.ShouldBe(GenericTestClass+".ShouldBeString<System.String>(\"A\")");
            shouldBeStringPassA.Messages.ShouldBeEmpty();
            shouldBeStringPassA.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);

            shouldBeStringPassBStart.ShouldBeExecutionTimeTest(GenericTestClass + ".ShouldBeString", assemblyPath);

            shouldBeStringPassB.TestCase.ShouldBeExecutionTimeTest(GenericTestClass+".ShouldBeString", assemblyPath);
            shouldBeStringPassB.TestCase.DisplayName.ShouldBe(GenericTestClass+".ShouldBeString");
            shouldBeStringPassB.Outcome.ShouldBe(TestOutcome.Passed);
            shouldBeStringPassB.ErrorMessage.ShouldBe(null);
            shouldBeStringPassB.ErrorStackTrace.ShouldBe(null);
            shouldBeStringPassB.DisplayName.ShouldBe(GenericTestClass+".ShouldBeString<System.String>(\"B\")");
            shouldBeStringPassB.Messages.ShouldBeEmpty();
            shouldBeStringPassB.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);


            shouldBeStringFailStart.ShouldBeExecutionTimeTest(GenericTestClass + ".ShouldBeString", assemblyPath);

            shouldBeStringFail.TestCase.ShouldBeExecutionTimeTest(GenericTestClass+".ShouldBeString", assemblyPath);
            shouldBeStringFail.TestCase.DisplayName.ShouldBe(GenericTestClass+".ShouldBeString");
            shouldBeStringFail.Outcome.ShouldBe(TestOutcome.Failed);
            shouldBeStringFail.ErrorMessage.Lines().ShouldBe(
                "Expected: System.String",
                "Actual:   System.Int32");
            shouldBeStringFail.ErrorStackTrace
                .Lines()
                .NormalizeStackTraceLines()
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