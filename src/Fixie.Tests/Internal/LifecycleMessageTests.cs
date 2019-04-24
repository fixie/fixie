namespace Fixie.Tests.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Assertions;
    using Fixie.Internal;
    using Fixie.Internal.Listeners;
    using static Utility;

    public class LifecycleMessageTests : MessagingTests
    {
        public void ShouldDescribeTestLifecycleMessagesEmittedDuringExecution()
        {
            var listener = new StubCaseCompletedListener();

            using (new RedirectedConsole())
                Run(listener);

            var assembly = typeof(LifecycleMessageTests).Assembly;

            var assemblyStarted = listener.AssemblyStarts.Single();
            assemblyStarted.Assembly.ShouldBe(assembly);

            var classStarted = listener.ClassStarts.Single();
            classStarted.Class.FullName.ShouldBe(FullName<MessagingTests>() + "+SampleTestClass");

            listener.Cases.Count.ShouldBe(5);

            var fail = (CaseFailed)listener.Cases[0];
            var failByAssertion = (CaseFailed)listener.Cases[1];
            var pass = (CasePassed)listener.Cases[2];
            var skipWithReason = (CaseSkipped)listener.Cases[3];
            var skipWithoutReason = (CaseSkipped)listener.Cases[4];

            pass.Name.ShouldBe(TestClass + ".Pass");
            pass.Class.FullName.ShouldBe(TestClass);
            pass.Method.Name.ShouldBe("Pass");
            pass.Output.Lines().ShouldBe("Console.Out: Pass", "Console.Error: Pass");
            pass.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);

            fail.Name.ShouldBe(TestClass + ".Fail");
            fail.Class.FullName.ShouldBe(TestClass);
            fail.Method.Name.ShouldBe("Fail");
            fail.Output.Lines().ShouldBe("Console.Out: Fail", "Console.Error: Fail");
            fail.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
            fail.Exception.ShouldBeType<FailureException>();
            fail.Exception.LiterateStackTrace()
                .CleanStackTraceLineNumbers()
                .ShouldBe(At("Fail()"));
            fail.Exception.Message.ShouldBe("'Fail' failed!");

            failByAssertion.Name.ShouldBe(TestClass + ".FailByAssertion");
            failByAssertion.Class.FullName.ShouldBe(TestClass);
            failByAssertion.Method.Name.ShouldBe("FailByAssertion");
            failByAssertion.Output.Lines().ShouldBe("Console.Out: FailByAssertion", "Console.Error: FailByAssertion");
            failByAssertion.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
            failByAssertion.Exception.ShouldBeType<ExpectedException>();
            failByAssertion.Exception.LiterateStackTrace()
                .CleanStackTraceLineNumbers()
                .ShouldBe(At("FailByAssertion()"));
            failByAssertion.Exception.Message.Lines().ShouldBe(
                "Expected: 2",
                "Actual:   1");

            skipWithReason.Name.ShouldBe(TestClass + ".SkipWithReason");
            skipWithReason.Class.FullName.ShouldBe(TestClass);
            skipWithReason.Method.Name.ShouldBe("SkipWithReason");
            skipWithReason.Output.ShouldBeEmpty();
            skipWithReason.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
            skipWithReason.Reason.ShouldBe("⚠ Skipped with reason.");

            skipWithoutReason.Name.ShouldBe(TestClass + ".SkipWithoutReason");
            skipWithoutReason.Class.FullName.ShouldBe(TestClass);
            skipWithoutReason.Method.Name.ShouldBe("SkipWithoutReason");
            skipWithoutReason.Output.ShouldBeEmpty();
            skipWithoutReason.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
            skipWithoutReason.Reason.ShouldBeNull();

            var classCompleted = listener.ClassCompletions.Single();
            classCompleted.Class.FullName.ShouldBe(FullName<MessagingTests>() + "+SampleTestClass");

            var assemblyCompleted = listener.AssemblyCompletions.Single();
            assemblyCompleted.Assembly.ShouldBe(assembly);
        }

        public class StubCaseCompletedListener :
            Handler<AssemblyStarted>,
            Handler<ClassStarted>,
            Handler<CaseCompleted>,
            Handler<ClassCompleted>,
            Handler<AssemblyCompleted>
        {
            public List<AssemblyStarted> AssemblyStarts { get; } = new List<AssemblyStarted>();
            public List<ClassStarted> ClassStarts { get; } = new List<ClassStarted>();
            public List<CaseCompleted> Cases { get; } = new List<CaseCompleted>();
            public List<ClassCompleted> ClassCompletions { get; } = new List<ClassCompleted>();
            public List<AssemblyCompleted> AssemblyCompletions { get; } = new List<AssemblyCompleted>();

            public void Handle(AssemblyStarted message) => AssemblyStarts.Add(message);
            public void Handle(ClassStarted message) => ClassStarts.Add(message);
            public void Handle(CaseCompleted message) => Cases.Add(message);
            public void Handle(ClassCompleted message) => ClassCompletions.Add(message);
            public void Handle(AssemblyCompleted message) => AssemblyCompletions.Add(message);
        }
    }
}
