namespace Fixie.Tests.Internal
{
    using System;
    using System.Collections.Generic;
    using Assertions;
    using Fixie.Internal;
    using Fixie.Internal.Listeners;
    using static Utility;

    public class LifecycleMessageTests : MessagingTests
    {
        public void ShouldDescribeTestLifecycleMessagesEmittedDuringExecution()
        {
            var assembly = typeof(LifecycleMessageTests).Assembly;
            var listener = new StubCaseCompletedListener();

            Run(listener, out _);

            listener.Messages.Count.ShouldBe(18);
            
            var assemblyStarted = (AssemblyStarted)listener.Messages[0];
            var sampleTestClassStarted = (ClassStarted)listener.Messages[1];
            var failStarted = (CaseStarted)listener.Messages[2];
            var fail = (CaseFailed)listener.Messages[3];
            var failByAssertionStarted = (CaseStarted)listener.Messages[4];
            var failByAssertion = (CaseFailed)listener.Messages[5];
            var passStarted = (CaseStarted)listener.Messages[6];
            var pass = (CasePassed)listener.Messages[7];
            
            var skipWithReason = (CaseSkipped)listener.Messages[8];
            var skipWithoutReason = (CaseSkipped)listener.Messages[9];
            var sampleTestClassCompleted = (ClassCompleted)listener.Messages[10];
            var sampleGenericTestClassStarted = (ClassStarted)listener.Messages[11];
            var shouldBeStringPassStarted = (CaseStarted)listener.Messages[12];
            var shouldBeStringPass = (CasePassed)listener.Messages[13];
            var shouldBeStringFailStarted = (CaseStarted)listener.Messages[14];
            var shouldBeStringFail = (CaseFailed)listener.Messages[15];
            var sampleGenericTestClassCompleted = (ClassCompleted)listener.Messages[16];
            var assemblyCompleted = (AssemblyCompleted)listener.Messages[17];

            assemblyStarted.Assembly.ShouldBe(assembly);
            
            sampleTestClassStarted.Class.FullName.ShouldBe(FullName<MessagingTests>() + "+SampleTestClass");

            passStarted.Test.Name.ShouldBe(TestClass + ".Pass");

            pass.Test.Name.ShouldBe(TestClass + ".Pass");
            pass.Name.ShouldBe(TestClass + ".Pass");
            pass.Output.Lines().ShouldBe("Console.Out: Pass", "Console.Error: Pass");
            pass.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);

            failStarted.Test.Name.ShouldBe(TestClass + ".Fail");

            fail.Test.Name.ShouldBe(TestClass + ".Fail");
            fail.Name.ShouldBe(TestClass + ".Fail");
            fail.Output.Lines().ShouldBe("Console.Out: Fail", "Console.Error: Fail");
            fail.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
            fail.Exception.ShouldBe<FailureException>();
            fail.Exception.LiterateStackTrace()
                .Lines()
                .CleanStackTraceLineNumbers()
                .ShouldBe(At("Fail()"));
            fail.Exception.Message.ShouldBe("'Fail' failed!");

            failByAssertionStarted.Test.Name.ShouldBe(TestClass + ".FailByAssertion");

            failByAssertion.Test.Name.ShouldBe(TestClass + ".FailByAssertion");
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

            skipWithReason.Test.Name.ShouldBe(TestClass + ".SkipWithReason");
            skipWithReason.Name.ShouldBe(TestClass + ".SkipWithReason");
            skipWithReason.Output.ShouldBe("");
            skipWithReason.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
            skipWithReason.Reason.ShouldBe("⚠ Skipped with reason.");

            skipWithoutReason.Test.Name.ShouldBe(TestClass + ".SkipWithoutReason");
            skipWithoutReason.Name.ShouldBe(TestClass + ".SkipWithoutReason");
            skipWithoutReason.Output.ShouldBe("");
            skipWithoutReason.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
            skipWithoutReason.Reason.ShouldBe(null);

            sampleTestClassCompleted.Class.FullName.ShouldBe(FullName<MessagingTests>() + "+SampleTestClass");

            sampleGenericTestClassStarted.Class.FullName.ShouldBe(FullName<MessagingTests>() + "+SampleGenericTestClass");

            shouldBeStringPassStarted.Test.Name.ShouldBe(GenericTestClass + ".ShouldBeString");

            shouldBeStringPass.Test.Name.ShouldBe(GenericTestClass + ".ShouldBeString");
            shouldBeStringPass.Name.ShouldBe(GenericTestClass + ".ShouldBeString<System.String>(\"abc\")");
            shouldBeStringPass.Output.ShouldBe("");
            shouldBeStringPass.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);

            shouldBeStringFailStarted.Test.Name.ShouldBe(GenericTestClass + ".ShouldBeString");

            shouldBeStringFail.Test.Name.ShouldBe(GenericTestClass + ".ShouldBeString");
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

            sampleGenericTestClassCompleted.Class.FullName.ShouldBe(FullName<MessagingTests>() + "+SampleGenericTestClass");

            assemblyCompleted.Assembly.ShouldBe(assembly);
        }

        public class StubCaseCompletedListener :
            Handler<AssemblyStarted>,
            Handler<ClassStarted>,
            Handler<CaseStarted>,
            Handler<CaseCompleted>,
            Handler<ClassCompleted>,
            Handler<AssemblyCompleted>
        {
            public List<object> Messages { get; } = new List<object>();

            public void Handle(AssemblyStarted message) => Messages.Add(message);
            public void Handle(ClassStarted message) => Messages.Add(message);
            public void Handle(CaseStarted message) => Messages.Add(message);
            public void Handle(CaseCompleted message) => Messages.Add(message);
            public void Handle(ClassCompleted message) => Messages.Add(message);
            public void Handle(AssemblyCompleted message) => Messages.Add(message);
        }
    }
}
