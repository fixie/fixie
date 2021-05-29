namespace Fixie.Tests.Reports
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Assertions;
    using Fixie.Reports;

    public class LifecycleMessageTests : MessagingTests
    {
        public async Task ShouldDescribeTestLifecycleMessagesEmittedDuringExecution()
        {
            var assembly = typeof(LifecycleMessageTests).Assembly;
            var report = new StubTestCompletedReport();

            await RunAsync(report);

            report.Messages.Count.ShouldBe(15);
            
            var assemblyStarted = (AssemblyStarted)report.Messages[0];
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
            var assemblyCompleted = (AssemblyCompleted)report.Messages[14];

            assemblyStarted.Assembly.ShouldBe(assembly);
            
            passStarted.Test.ShouldBe(TestClass + ".Pass");

            pass.Test.ShouldBe(TestClass + ".Pass");
            pass.Name.ShouldBe(TestClass + ".Pass");
            pass.Output.Lines().ShouldBe("Standard Out: Pass");
            pass.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);

            failStarted.Test.ShouldBe(TestClass + ".Fail");

            fail.Test.ShouldBe(TestClass + ".Fail");
            fail.Name.ShouldBe(TestClass + ".Fail");
            fail.Output.Lines().ShouldBe("Standard Out: Fail");
            fail.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
            fail.Reason.ShouldBe<FailureException>();
            fail.Reason.LiterateStackTrace()
                .Lines()
                .CleanStackTraceLineNumbers()
                .ShouldBe(At("Fail()"));
            fail.Reason.Message.ShouldBe("'Fail' failed!");

            failByAssertionStarted.Test.ShouldBe(TestClass + ".FailByAssertion");

            failByAssertion.Test.ShouldBe(TestClass + ".FailByAssertion");
            failByAssertion.Name.ShouldBe(TestClass + ".FailByAssertion");
            failByAssertion.Output.Lines().ShouldBe("Standard Out: FailByAssertion");
            failByAssertion.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
            failByAssertion.Reason.ShouldBe<AssertException>();
            failByAssertion.Reason.LiterateStackTrace()
                .Lines()
                .CleanStackTraceLineNumbers()
                .ShouldBe(At("FailByAssertion()"));
            failByAssertion.Reason.Message.Lines().ShouldBe(
                "Expected: 2",
                "Actual:   1");

            skip.Test.ShouldBe(TestClass + ".Skip");
            skip.Name.ShouldBe(TestClass + ".Skip");
            skip.Output.ShouldBe("");
            skip.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
            skip.Reason.ShouldBe("⚠ Skipped with attribute.");

            shouldBeStringPassAStarted.Test.ShouldBe(GenericTestClass + ".ShouldBeString");

            shouldBeStringPassA.Test.ShouldBe(GenericTestClass + ".ShouldBeString");
            shouldBeStringPassA.Name.ShouldBe(GenericTestClass + ".ShouldBeString<System.String>(\"A\")");
            shouldBeStringPassA.Output.ShouldBe("");
            shouldBeStringPassA.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);

            shouldBeStringPassBStarted.Test.ShouldBe(GenericTestClass + ".ShouldBeString");

            shouldBeStringPassB.Test.ShouldBe(GenericTestClass + ".ShouldBeString");
            shouldBeStringPassB.Name.ShouldBe(GenericTestClass + ".ShouldBeString<System.String>(\"B\")");
            shouldBeStringPassB.Output.ShouldBe("");
            shouldBeStringPassB.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);

            shouldBeStringFailStarted.Test.ShouldBe(GenericTestClass + ".ShouldBeString");

            shouldBeStringFail.Test.ShouldBe(GenericTestClass + ".ShouldBeString");
            shouldBeStringFail.Name.ShouldBe(GenericTestClass + ".ShouldBeString<System.Int32>(123)");
            shouldBeStringFail.Output.ShouldBe("");
            shouldBeStringFail.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
            shouldBeStringFail.Reason.ShouldBe<AssertException>();
            shouldBeStringFail.Reason.LiterateStackTrace()
                .Lines()
                .CleanStackTraceLineNumbers()
                .ShouldBe(At<SampleGenericTestClass>("ShouldBeString[T](T genericArgument)"));
            shouldBeStringFail.Reason.Message.Lines().ShouldBe(
                "Expected: System.String",
                "Actual:   System.Int32");

            assemblyCompleted.Assembly.ShouldBe(assembly);
        }

        public class StubTestCompletedReport :
            Handler<AssemblyStarted>,
            Handler<TestStarted>,
            Handler<TestCompleted>,
            Handler<AssemblyCompleted>
        {
            public List<object> Messages { get; } = new List<object>();

            public void Handle(AssemblyStarted message) => Messages.Add(message);
            public void Handle(TestStarted message) => Messages.Add(message);
            public void Handle(TestCompleted message) => Messages.Add(message);
            public void Handle(AssemblyCompleted message) => Messages.Add(message);
        }
    }
}
