using Fixie.TestAdapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Fixie.Internal;
using Fixie.Tests.Reports;
using static System.Environment;

namespace Fixie.Tests.TestAdapter;

using TestResult = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestResult;

public class VsExecutionRecorderTests : MessagingTests
{
    public void ShouldMapMessagesToVsTestExecutionRecorder()
    {
        const string assemblyPath = "assembly.path.dll";
        var recorder = new StubExecutionRecorder();

        var vsExecutionRecorder = new VsExecutionRecorder(recorder, assemblyPath);

        RecordAnticipatedPipeMessages(vsExecutionRecorder);

        var messages = recorder.Messages;

        messages.Count.ShouldBe(13);

        foreach (var message in messages)
        {
            if (message is TestResult result)
            {
                result.Traits.ShouldBe([]);
                result.Attachments.ShouldBe([]);
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
            .ShouldBe(["Fixie.Tests.FailureException", At("Fail()")]);
        fail.DisplayName.ShouldBe(TestClass+".Fail");
        fail.Messages.ShouldBe([]);
        fail.Duration.ShouldBe(TimeSpan.FromMilliseconds(102));

        failByAssertionStart.ShouldBeExecutionTimeTest(TestClass + ".FailByAssertion", assemblyPath);

        failByAssertion.TestCase.ShouldBeExecutionTimeTest(TestClass+".FailByAssertion", assemblyPath);
        failByAssertion.TestCase.DisplayName.ShouldBe(TestClass+".FailByAssertion");
        failByAssertion.Outcome.ShouldBe(TestOutcome.Failed);
        failByAssertion.ErrorMessage.ShouldBe("x should be 2 but was 1");
        failByAssertion.ErrorStackTrace
            .Lines()
            .NormalizeStackTraceLines()
            .ShouldBe(["Fixie.Tests.Assertions.AssertException", At("FailByAssertion()")]);
        failByAssertion.DisplayName.ShouldBe(TestClass+".FailByAssertion");
        failByAssertion.Messages.ShouldBe([]);
        failByAssertion.Duration.ShouldBe(TimeSpan.FromMilliseconds(103));

        passStart.ShouldBeExecutionTimeTest(TestClass + ".Pass", assemblyPath);

        pass.TestCase.ShouldBeExecutionTimeTest(TestClass+".Pass", assemblyPath);
        pass.TestCase.DisplayName.ShouldBe(TestClass+".Pass");
        pass.Outcome.ShouldBe(TestOutcome.Passed);
        pass.ErrorMessage.ShouldBe(null);
        pass.ErrorStackTrace.ShouldBe(null);
        pass.DisplayName.ShouldBe(TestClass+".Pass");
        pass.Messages.ShouldBe([]);
        pass.Duration.ShouldBe(TimeSpan.FromMilliseconds(104));

        skip.TestCase.ShouldBeExecutionTimeTest(TestClass+".Skip", assemblyPath);
        skip.TestCase.DisplayName.ShouldBe(TestClass+".Skip");
        skip.Outcome.ShouldBe(TestOutcome.Skipped);
        skip.ErrorMessage.ShouldBe("⚠ Skipped with attribute.");
        skip.ErrorStackTrace.ShouldBe(null);
        skip.DisplayName.ShouldBe(TestClass+".Skip");
        skip.Messages.ShouldBe([]);
        skip.Duration.ShouldBe(TimeSpan.Zero);

        shouldBeStringPassAStart.ShouldBeExecutionTimeTest(GenericTestClass + ".ShouldBeString", assemblyPath);

        shouldBeStringPassA.TestCase.ShouldBeExecutionTimeTest(GenericTestClass+".ShouldBeString", assemblyPath);
        shouldBeStringPassA.TestCase.DisplayName.ShouldBe(GenericTestClass+".ShouldBeString");
        shouldBeStringPassA.Outcome.ShouldBe(TestOutcome.Passed);
        shouldBeStringPassA.ErrorMessage.ShouldBe(null);
        shouldBeStringPassA.ErrorStackTrace.ShouldBe(null);
        shouldBeStringPassA.DisplayName.ShouldBe(GenericTestClass+".ShouldBeString<System.String>(\"A\")");
        shouldBeStringPassA.Messages.ShouldBe([]);
        shouldBeStringPassA.Duration.ShouldBe(TimeSpan.FromMilliseconds(105));

        shouldBeStringPassBStart.ShouldBeExecutionTimeTest(GenericTestClass + ".ShouldBeString", assemblyPath);

        shouldBeStringPassB.TestCase.ShouldBeExecutionTimeTest(GenericTestClass+".ShouldBeString", assemblyPath);
        shouldBeStringPassB.TestCase.DisplayName.ShouldBe(GenericTestClass+".ShouldBeString");
        shouldBeStringPassB.Outcome.ShouldBe(TestOutcome.Passed);
        shouldBeStringPassB.ErrorMessage.ShouldBe(null);
        shouldBeStringPassB.ErrorStackTrace.ShouldBe(null);
        shouldBeStringPassB.DisplayName.ShouldBe(GenericTestClass+".ShouldBeString<System.String>(\"B\")");
        shouldBeStringPassB.Messages.ShouldBe([]);
        shouldBeStringPassB.Duration.ShouldBe(TimeSpan.FromMilliseconds(106));

        shouldBeStringFailStart.ShouldBeExecutionTimeTest(GenericTestClass + ".ShouldBeString", assemblyPath);

        shouldBeStringFail.TestCase.ShouldBeExecutionTimeTest(GenericTestClass+".ShouldBeString", assemblyPath);
        shouldBeStringFail.TestCase.DisplayName.ShouldBe(GenericTestClass+".ShouldBeString");
        shouldBeStringFail.Outcome.ShouldBe(TestOutcome.Failed);
        shouldBeStringFail.ErrorMessage.ShouldBe("genericArgument should be System.String but was System.Int32");
        shouldBeStringFail.ErrorStackTrace
            .Lines()
            .NormalizeStackTraceLines()
            .ShouldBe([
                "Fixie.Tests.Assertions.AssertException",
                At<SampleGenericTestClass>("ShouldBeString[T](T genericArgument)")
            ]);
        shouldBeStringFail.DisplayName.ShouldBe(GenericTestClass+".ShouldBeString<System.Int32>(123)");
        shouldBeStringFail.Messages.ShouldBe([]);
        shouldBeStringFail.Duration.ShouldBe(TimeSpan.FromMilliseconds(107));
    }

    void RecordAnticipatedPipeMessages(VsExecutionRecorder vsExecutionRecorder)
    {
        vsExecutionRecorder.Record(Deserialized(new PipeMessage.TestStarted
        {
            Test = TestClass + ".Fail"
        }));

        vsExecutionRecorder.Record(Deserialized(new PipeMessage.TestFailed
        {
            Test = TestClass + ".Fail",
            TestCase = TestClass + ".Fail",
            DurationInMilliseconds = 102,
            Reason = new PipeMessage.Exception
            {
                Type = "Fixie.Tests.FailureException",
                Message = "'Fail' failed!",
                StackTrace = At("Fail()")
            }
        }));

        vsExecutionRecorder.Record(Deserialized(new PipeMessage.TestStarted
        {
            Test = TestClass + ".FailByAssertion"
        }));

        vsExecutionRecorder.Record(Deserialized(new PipeMessage.TestFailed
        {
            Test = TestClass + ".FailByAssertion",
            TestCase = TestClass + ".FailByAssertion",
            DurationInMilliseconds = 103,
            Reason = new PipeMessage.Exception
            {
                Type = "Fixie.Tests.Assertions.AssertException",
                Message = "x should be 2 but was 1",
                StackTrace = At("FailByAssertion()")
            }
        }));
        
        vsExecutionRecorder.Record(Deserialized(new PipeMessage.TestStarted
        {
            Test = TestClass + ".Pass"
        }));
        
        vsExecutionRecorder.Record(Deserialized(new PipeMessage.TestPassed
        {
            Test = TestClass+".Pass",
            TestCase = TestClass+".Pass",
            DurationInMilliseconds = 104
        }));

        vsExecutionRecorder.Record(Deserialized(new PipeMessage.TestSkipped
        {
            Test =TestClass+".Skip",
            TestCase = TestClass+".Skip",
            DurationInMilliseconds = 0,
            Reason = "⚠ Skipped with attribute."
        }));
        
        vsExecutionRecorder.Record(Deserialized(new PipeMessage.TestStarted
        {
            Test = GenericTestClass + ".ShouldBeString"
        }));
        
        vsExecutionRecorder.Record(Deserialized(new PipeMessage.TestPassed
        {
            Test = GenericTestClass+".ShouldBeString",
            TestCase = GenericTestClass+".ShouldBeString<System.String>(\"A\")",
            DurationInMilliseconds = 105
        }));
        
        vsExecutionRecorder.Record(Deserialized(new PipeMessage.TestStarted
        {
            Test = GenericTestClass + ".ShouldBeString"
        }));
        
        vsExecutionRecorder.Record(Deserialized(new PipeMessage.TestPassed
        {
            Test = GenericTestClass+".ShouldBeString",
            TestCase = GenericTestClass+".ShouldBeString<System.String>(\"B\")",
            DurationInMilliseconds = 106
        }));
        
        vsExecutionRecorder.Record(Deserialized(new PipeMessage.TestStarted
        {
            Test = GenericTestClass + ".ShouldBeString"
        }));
        
        vsExecutionRecorder.Record(Deserialized(new PipeMessage.TestFailed
        {
            Test = GenericTestClass+".ShouldBeString",
            TestCase = GenericTestClass+".ShouldBeString<System.Int32>(123)",
            DurationInMilliseconds = 107,
            Reason = new PipeMessage.Exception
            {
                Type = "Fixie.Tests.Assertions.AssertException",
                Message = "genericArgument should be System.String but was System.Int32",
                StackTrace = At<SampleGenericTestClass>("ShouldBeString[T](T genericArgument)")
            }
        }));
    }

    static T Deserialized<T>(T original)
    {
        // Because the inter-process communication between the VsTest process
        // and the test assembly process is not exercised in these single-process
        // tests, put a given sample message through the same serialization round
        // trip that would be applied at runtime, in order to detect data loss.

        return PipeMessage.Deserialize<T>(PipeMessage.Serialize(original));
    }

    class StubExecutionRecorder : ITestExecutionRecorder
    {
        public List<object> Messages { get; } = [];

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