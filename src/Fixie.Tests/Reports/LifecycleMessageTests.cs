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
            var report = new StubCaseCompletedReport();

            await RunAsync(report);

            report.Messages.Count.ShouldBe(14);
            
            var assemblyStarted = (AssemblyStarted)report.Messages[0];
            var failStarted = (CaseStarted)report.Messages[1];
            var fail = (CaseFailed)report.Messages[2];
            var failByAssertionStarted = (CaseStarted)report.Messages[3];
            var failByAssertion = (CaseFailed)report.Messages[4];
            var passStarted = (CaseStarted)report.Messages[5];
            var pass = (CasePassed)report.Messages[6];
            var skipWithReason = (CaseSkipped)report.Messages[7];
            var skipWithoutReason = (CaseSkipped)report.Messages[8];
            var shouldBeStringPassStarted = (CaseStarted)report.Messages[9];
            var shouldBeStringPass = (CasePassed)report.Messages[10];
            var shouldBeStringFailStarted = (CaseStarted)report.Messages[11];
            var shouldBeStringFail = (CaseFailed)report.Messages[12];
            var assemblyCompleted = (AssemblyCompleted)report.Messages[13];

            assemblyStarted.Assembly.ShouldBe(assembly);
            
            passStarted.Test.ShouldBe(TestClass + ".Pass");

            pass.Test.FullName.ShouldBe(TestClass + ".Pass");
            pass.Name.ShouldBe(TestClass + ".Pass");
            pass.Output.Lines().ShouldBe("Console.Out: Pass", "Console.Error: Pass");
            pass.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);

            failStarted.Test.ShouldBe(TestClass + ".Fail");

            fail.Test.FullName.ShouldBe(TestClass + ".Fail");
            fail.Name.ShouldBe(TestClass + ".Fail");
            fail.Output.Lines().ShouldBe("Console.Out: Fail", "Console.Error: Fail");
            fail.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
            fail.Exception.ShouldBe<FailureException>();
            fail.Exception.LiterateStackTrace()
                .Lines()
                .CleanStackTraceLineNumbers()
                .ShouldBe(At("Fail()"));
            fail.Exception.Message.ShouldBe("'Fail' failed!");

            failByAssertionStarted.Test.ShouldBe(TestClass + ".FailByAssertion");

            failByAssertion.Test.FullName.ShouldBe(TestClass + ".FailByAssertion");
            failByAssertion.Name.ShouldBe(TestClass + ".FailByAssertion");
            failByAssertion.Output.Lines().ShouldBe("Console.Out: FailByAssertion", "Console.Error: FailByAssertion");
            failByAssertion.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
            failByAssertion.Exception.ShouldBe<AssertException>();
            failByAssertion.Exception.LiterateStackTrace()
                .Lines()
                .CleanStackTraceLineNumbers()
                .ShouldBe(At("FailByAssertion()"));
            failByAssertion.Exception.Message.Lines().ShouldBe(
                "Expected: 2",
                "Actual:   1");

            skipWithReason.Test.FullName.ShouldBe(TestClass + ".SkipWithReason");
            skipWithReason.Name.ShouldBe(TestClass + ".SkipWithReason");
            skipWithReason.Output.ShouldBe("");
            skipWithReason.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
            skipWithReason.Reason.ShouldBe("⚠ Skipped with reason.");

            skipWithoutReason.Test.FullName.ShouldBe(TestClass + ".SkipWithoutReason");
            skipWithoutReason.Name.ShouldBe(TestClass + ".SkipWithoutReason");
            skipWithoutReason.Output.ShouldBe("");
            skipWithoutReason.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
            skipWithoutReason.Reason.ShouldBe(null);

            shouldBeStringPassStarted.Test.ShouldBe(GenericTestClass + ".ShouldBeString");

            shouldBeStringPass.Test.FullName.ShouldBe(GenericTestClass + ".ShouldBeString");
            shouldBeStringPass.Name.ShouldBe(GenericTestClass + ".ShouldBeString<System.String>(\"abc\")");
            shouldBeStringPass.Output.ShouldBe("");
            shouldBeStringPass.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);

            shouldBeStringFailStarted.Test.ShouldBe(GenericTestClass + ".ShouldBeString");

            shouldBeStringFail.Test.FullName.ShouldBe(GenericTestClass + ".ShouldBeString");
            shouldBeStringFail.Name.ShouldBe(GenericTestClass + ".ShouldBeString<System.Int32>(123)");
            shouldBeStringFail.Output.ShouldBe("");
            shouldBeStringFail.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
            shouldBeStringFail.Exception.ShouldBe<AssertException>();
            shouldBeStringFail.Exception.LiterateStackTrace()
                .Lines()
                .CleanStackTraceLineNumbers()
                .ShouldBe(At<SampleGenericTestClass>("ShouldBeString[T](T genericArgument)"));
            shouldBeStringFail.Exception.Message.Lines().ShouldBe(
                "Expected: System.String",
                "Actual:   System.Int32");

            assemblyCompleted.Assembly.ShouldBe(assembly);
        }

        public class StubCaseCompletedReport :
            Handler<AssemblyStarted>,
            Handler<CaseStarted>,
            Handler<CaseCompleted>,
            Handler<AssemblyCompleted>
        {
            public List<object> Messages { get; } = new List<object>();

            public void Handle(AssemblyStarted message) => Messages.Add(message);
            public void Handle(CaseStarted message) => Messages.Add(message);
            public void Handle(CaseCompleted message) => Messages.Add(message);
            public void Handle(AssemblyCompleted message) => Messages.Add(message);
        }
    }
}
