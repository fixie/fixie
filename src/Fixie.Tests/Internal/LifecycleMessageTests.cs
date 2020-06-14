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

            listener.Messages.Count.ShouldBe(20);
            
            var assemblyStarted = (AssemblyStarted)listener.Messages[0];
            var sampleTestClassStarted = (ClassStarted)listener.Messages[1];
            var failStarted = (CaseStarted)listener.Messages[2];
            var fail = (CaseFailed)listener.Messages[3];
            var failByAssertionStarted = (CaseStarted)listener.Messages[4];
            var failByAssertion = (CaseFailed)listener.Messages[5];
            var passStarted = (CaseStarted)listener.Messages[6];
            var pass = (CasePassed)listener.Messages[7];
            var skipWithReasonStarted = (CaseStarted)listener.Messages[8];
            var skipWithReason = (CaseSkipped)listener.Messages[9];
            var skipWithoutReasonStarted = (CaseStarted)listener.Messages[10];
            var skipWithoutReason = (CaseSkipped)listener.Messages[11];
            var sampleTestClassCompleted = (ClassCompleted)listener.Messages[12];
            var sampleGenericTestClassStarted = (ClassStarted)listener.Messages[13];
            var shouldBeStringPassStarted = (CaseStarted)listener.Messages[14];
            var shouldBeStringPass = (CasePassed)listener.Messages[15];
            var shouldBeStringFailStarted = (CaseStarted)listener.Messages[16];
            var shouldBeStringFail = (CaseFailed)listener.Messages[17];
            var sampleGenericTestClassCompleted = (ClassCompleted)listener.Messages[18];
            var assemblyCompleted = (AssemblyCompleted)listener.Messages[19];

            assemblyStarted.Assembly.ShouldBe(assembly);
            
            sampleTestClassStarted.Class.FullName.ShouldBe(FullName<MessagingTests>() + "+SampleTestClass");
            
            passStarted.Name.ShouldBe(TestClass + ".Pass");
            passStarted.Class.FullName.ShouldBe(TestClass);
            passStarted.Method.Name.ShouldBe("Pass");

            pass.Name.ShouldBe(TestClass + ".Pass");
            pass.Class.FullName.ShouldBe(TestClass);
            pass.Method.Name.ShouldBe("Pass");
            pass.Output.Lines().ShouldBe("Console.Out: Pass", "Console.Error: Pass");
            pass.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);

            failStarted.Name.ShouldBe(TestClass + ".Fail");
            failStarted.Class.FullName.ShouldBe(TestClass);
            failStarted.Method.Name.ShouldBe("Fail");
            
            fail.Name.ShouldBe(TestClass + ".Fail");
            fail.Class.FullName.ShouldBe(TestClass);
            fail.Method.Name.ShouldBe("Fail");
            fail.Output.Lines().ShouldBe("Console.Out: Fail", "Console.Error: Fail");
            fail.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
            fail.Exception.ShouldBe<FailureException>();
            fail.Exception.LiterateStackTrace()
                .Lines()
                .CleanStackTraceLineNumbers()
                .ShouldBe(At("Fail()"));
            fail.Exception.Message.ShouldBe("'Fail' failed!");

            failByAssertionStarted.Name.ShouldBe(TestClass + ".FailByAssertion");
            failByAssertionStarted.Class.FullName.ShouldBe(TestClass);
            failByAssertionStarted.Method.Name.ShouldBe("FailByAssertion");
            
            failByAssertion.Name.ShouldBe(TestClass + ".FailByAssertion");
            failByAssertion.Class.FullName.ShouldBe(TestClass);
            failByAssertion.Method.Name.ShouldBe("FailByAssertion");
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

            skipWithReasonStarted.Name.ShouldBe(TestClass + ".SkipWithReason");
            skipWithReasonStarted.Class.FullName.ShouldBe(TestClass);
            skipWithReasonStarted.Method.Name.ShouldBe("SkipWithReason");
            
            skipWithReason.Name.ShouldBe(TestClass + ".SkipWithReason");
            skipWithReason.Class.FullName.ShouldBe(TestClass);
            skipWithReason.Method.Name.ShouldBe("SkipWithReason");
            skipWithReason.Output.ShouldBe("");
            skipWithReason.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
            skipWithReason.Reason.ShouldBe("⚠ Skipped with reason.");

            skipWithoutReasonStarted.Name.ShouldBe(TestClass + ".SkipWithoutReason");
            skipWithoutReasonStarted.Class.FullName.ShouldBe(TestClass);
            skipWithoutReasonStarted.Method.Name.ShouldBe("SkipWithoutReason");
            
            skipWithoutReason.Name.ShouldBe(TestClass + ".SkipWithoutReason");
            skipWithoutReason.Class.FullName.ShouldBe(TestClass);
            skipWithoutReason.Method.Name.ShouldBe("SkipWithoutReason");
            skipWithoutReason.Output.ShouldBe("");
            skipWithoutReason.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
            skipWithoutReason.Reason.ShouldBe(null);

            sampleTestClassCompleted.Class.FullName.ShouldBe(FullName<MessagingTests>() + "+SampleTestClass");

            sampleGenericTestClassStarted.Class.FullName.ShouldBe(FullName<MessagingTests>() + "+SampleGenericTestClass");

            shouldBeStringPassStarted.Name.ShouldBe(GenericTestClass + ".ShouldBeString<System.String>(\"abc\")");
            shouldBeStringPassStarted.Class.FullName.ShouldBe(GenericTestClass);
            shouldBeStringPassStarted.Method.Name.ShouldBe("ShouldBeString");
            
            shouldBeStringPass.Name.ShouldBe(GenericTestClass + ".ShouldBeString<System.String>(\"abc\")");
            shouldBeStringPass.Class.FullName.ShouldBe(GenericTestClass);
            shouldBeStringPass.Method.Name.ShouldBe("ShouldBeString");
            shouldBeStringPass.Output.ShouldBe("");
            shouldBeStringPass.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);

            shouldBeStringFailStarted.Name.ShouldBe(GenericTestClass + ".ShouldBeString<System.Int32>(123)");
            shouldBeStringFailStarted.Class.FullName.ShouldBe(GenericTestClass);
            shouldBeStringFailStarted.Method.Name.ShouldBe("ShouldBeString");
            
            shouldBeStringFail.Name.ShouldBe(GenericTestClass + ".ShouldBeString<System.Int32>(123)");
            shouldBeStringFail.Class.FullName.ShouldBe(GenericTestClass);
            shouldBeStringFail.Method.Name.ShouldBe("ShouldBeString");
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
