using Fixie.Reports;
using Fixie.Assertions;
namespace Fixie.Tests.Reports;

public class LifecycleMessageTests : MessagingTests
{
    public async Task ShouldDescribeTestLifecycleMessagesEmittedDuringExecution()
    {
        var report = new StubTestCompletedReport();

        await Run(report);

        report.Messages.Count.ShouldBe(15);
        
        var executionStarted = (ExecutionStarted)report.Messages[0];
        var failStarted = (TestStarted)report.Messages[1];
        var fail = (TestFailed)report.Messages[2];
        var failByAssertionStarted = (TestStarted)report.Messages[3];
        var failByAssertion = (TestFailed)report.Messages[4];
        var passStarted = (TestStarted)report.Messages[5];
        var pass = (TestPassed)report.Messages[6];
        var skip = (TestSkipped)report.Messages[7];
        var shouldBeStringPassAStarted = (TestStarted)report.Messages[8];
        var shouldBeStringPassA = (TestPassed)report.Messages[9];
        var shouldBeStringPassBStarted = (TestStarted)report.Messages[10];
        var shouldBeStringPassB = (TestPassed)report.Messages[11];
        var shouldBeStringFailStarted = (TestStarted)report.Messages[12];
        var shouldBeStringFail = (TestFailed)report.Messages[13];
        var executionCompleted = (ExecutionCompleted)report.Messages[14];

        executionStarted.ShouldNotBeNull();
        
        passStarted.Test.ShouldBe(TestClass + ".Pass");

        pass.Test.ShouldBe(TestClass + ".Pass");
        pass.TestCase.ShouldBe(TestClass + ".Pass");
        pass.Duration.Should(x => x >= TimeSpan.Zero);

        failStarted.Test.ShouldBe(TestClass + ".Fail");

        fail.Test.ShouldBe(TestClass + ".Fail");
        fail.TestCase.ShouldBe(TestClass + ".Fail");
        fail.Duration.Should(x => x >= TimeSpan.Zero);
        fail.Reason.ShouldBe<FailureException>();
        fail.Reason.StackTraceSummary()
            .ShouldBeStackTrace([At("Fail()")]);
        fail.Reason.Message.ShouldBe("'Fail' failed!");

        failByAssertionStarted.Test.ShouldBe(TestClass + ".FailByAssertion");

        failByAssertion.Test.ShouldBe(TestClass + ".FailByAssertion");
        failByAssertion.TestCase.ShouldBe(TestClass + ".FailByAssertion");
        failByAssertion.Duration.Should(x => x >= TimeSpan.Zero);
        failByAssertion.Reason.ShouldBe<AssertException>();
        failByAssertion.Reason.StackTraceSummary()
            .ShouldBeStackTrace([At("FailByAssertion()")]);
        failByAssertion.Reason.Message.ShouldBe("x should be 2 but was 1");

        skip.Test.ShouldBe(TestClass + ".Skip");
        skip.TestCase.ShouldBe(TestClass + ".Skip");
        skip.Duration.Should(x => x >= TimeSpan.Zero);
        skip.Reason.ShouldBe("⚠ Skipped with attribute.");

        shouldBeStringPassAStarted.Test.ShouldBe(GenericTestClass + ".ShouldBeString");

        shouldBeStringPassA.Test.ShouldBe(GenericTestClass + ".ShouldBeString");
        shouldBeStringPassA.TestCase.ShouldBe(GenericTestClass + ".ShouldBeString<System.String>(\"A\")");
        shouldBeStringPassA.Duration.Should(x => x >= TimeSpan.Zero);

        shouldBeStringPassBStarted.Test.ShouldBe(GenericTestClass + ".ShouldBeString");

        shouldBeStringPassB.Test.ShouldBe(GenericTestClass + ".ShouldBeString");
        shouldBeStringPassB.TestCase.ShouldBe(GenericTestClass + ".ShouldBeString<System.String>(\"B\")");
        shouldBeStringPassB.Duration.Should(x => x >= TimeSpan.Zero);

        shouldBeStringFailStarted.Test.ShouldBe(GenericTestClass + ".ShouldBeString");

        shouldBeStringFail.Test.ShouldBe(GenericTestClass + ".ShouldBeString");
        shouldBeStringFail.TestCase.ShouldBe(GenericTestClass + ".ShouldBeString<System.Int32>(123)");
        shouldBeStringFail.Duration.Should(x => x >= TimeSpan.Zero);
        shouldBeStringFail.Reason.ShouldBe<AssertException>();
        shouldBeStringFail.Reason.StackTraceSummary()
            .ShouldBeStackTrace([At<SampleGenericTestClass>("ShouldBeString[T](T genericArgument)")]);
        shouldBeStringFail.Reason.Message.ShouldBe("genericArgument should be typeof(string) but was typeof(int)");

        executionCompleted.Duration.Should(x => x >= TimeSpan.Zero);
        executionCompleted.Failed.ShouldBe(3);
        executionCompleted.Skipped.ShouldBe(1);
        executionCompleted.Passed.ShouldBe(3);
        executionCompleted.Total.ShouldBe(7);
    }

    public class StubTestCompletedReport :
        IHandler<ExecutionStarted>,
        IHandler<TestStarted>,
        IHandler<TestCompleted>,
        IHandler<ExecutionCompleted>
    {
        public List<object> Messages { get; } = [];

        public Task Handle(ExecutionStarted message)
        {
            Messages.Add(message);
            return Task.CompletedTask;
        }

        public Task Handle(TestStarted message)
        {
            Messages.Add(message);
            return Task.CompletedTask;
        }

        public Task Handle(TestCompleted message)
        {
            Messages.Add(message);
            return Task.CompletedTask;
        }

        public Task Handle(ExecutionCompleted message)
        {
            Messages.Add(message);
            return Task.CompletedTask;
        }
    }
}