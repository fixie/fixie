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

            report.Messages.Count.ShouldBe(14);
            
            var assemblyStarted = (AssemblyStarted)report.Messages[0];
            var failStarted = (TestStarted)report.Messages[1];
            var fail = (TestFailed)report.Messages[2];
            var failByAssertionStarted = (TestStarted)report.Messages[3];
            var failByAssertion = (TestFailed)report.Messages[4];
            var passStarted = (TestStarted)report.Messages[5];
            var pass = (TestPassed)report.Messages[6];
            var skipWithReason = (TestSkipped)report.Messages[7];
            var skipWithoutReason = (TestSkipped)report.Messages[8];
            var shouldBeStringPassStarted = (TestStarted)report.Messages[9];
            var shouldBeStringPass = (TestPassed)report.Messages[10];
            var shouldBeStringFailStarted = (TestStarted)report.Messages[11];
            var shouldBeStringFail = (TestFailed)report.Messages[12];
            var assemblyCompleted = (AssemblyCompleted)report.Messages[13];

            assemblyStarted.Assembly.ShouldBe(assembly);
            
            passStarted.Test.ShouldBe(TestClass + ".Pass");

            pass.Test.ShouldBe(TestClass + ".Pass");
            pass.Name.ShouldBe(TestClass + ".Pass");
            pass.Output.Lines().ShouldBe("Console.Out: Pass", "Console.Error: Pass");
            pass.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);

            failStarted.Test.ShouldBe(TestClass + ".Fail");

            fail.Test.ShouldBe(TestClass + ".Fail");
            fail.Name.ShouldBe(TestClass + ".Fail");
            fail.Output.Lines().ShouldBe("Console.Out: Fail", "Console.Error: Fail");
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
            failByAssertion.Output.Lines().ShouldBe("Console.Out: FailByAssertion", "Console.Error: FailByAssertion");
            failByAssertion.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
            failByAssertion.Reason.ShouldBe<AssertException>();
            failByAssertion.Reason.LiterateStackTrace()
                .Lines()
                .CleanStackTraceLineNumbers()
                .ShouldBe(At("FailByAssertion()"));
            failByAssertion.Reason.Message.Lines().ShouldBe(
                "Expected: 2",
                "Actual:   1");

            skipWithReason.Test.ShouldBe(TestClass + ".SkipWithReason");
            skipWithReason.Name.ShouldBe(TestClass + ".SkipWithReason");
            skipWithReason.Output.ShouldBe("");
            skipWithReason.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
            skipWithReason.Reason.ShouldBe("⚠ Skipped with reason.");

            skipWithoutReason.Test.ShouldBe(TestClass + ".SkipWithoutReason");
            skipWithoutReason.Name.ShouldBe(TestClass + ".SkipWithoutReason");
            skipWithoutReason.Output.ShouldBe("");
            skipWithoutReason.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
            skipWithoutReason.Reason.ShouldBe(null);

            shouldBeStringPassStarted.Test.ShouldBe(GenericTestClass + ".ShouldBeString");

            shouldBeStringPass.Test.ShouldBe(GenericTestClass + ".ShouldBeString");
            shouldBeStringPass.Name.ShouldBe(GenericTestClass + ".ShouldBeString<System.String>(\"abc\")");
            shouldBeStringPass.Output.ShouldBe("");
            shouldBeStringPass.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);

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
