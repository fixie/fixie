namespace Fixie.Tests.Execution
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Assertions;
    using Fixie.Execution;
    using Fixie.Execution.Listeners;
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
            assemblyStarted.Assembly.ShouldEqual(assembly);

            var classStarted = listener.ClassStarts.Single();
            classStarted.Class.FullName.ShouldEqual(FullName<MessagingTests>() + "+SampleTestClass");

            listener.Cases.Count.ShouldEqual(5);

            var fail = (CaseFailed)listener.Cases[0];
            var failByAssertion = (CaseFailed)listener.Cases[1];
            var pass = (CasePassed)listener.Cases[2];
            var skipWithReason = (CaseSkipped)listener.Cases[3];
            var skipWithoutReason = (CaseSkipped)listener.Cases[4];

            pass.Name.ShouldEqual(TestClass + ".Pass");
            pass.Class.FullName.ShouldEqual(TestClass);
            pass.Method.Name.ShouldEqual("Pass");
            pass.Output.Lines().ShouldEqual("Console.Out: Pass", "Console.Error: Pass");
            pass.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);

            fail.Name.ShouldEqual(TestClass + ".Fail");
            fail.Class.FullName.ShouldEqual(TestClass);
            fail.Method.Name.ShouldEqual("Fail");
            fail.Output.Lines().ShouldEqual("Console.Out: Fail", "Console.Error: Fail");
            fail.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
            fail.Exception.ShouldBeType<FailureException>();
            fail.Exception.CompoundStackTrace()
                .CleanStackTraceLineNumbers()
                .ShouldEqual(At("Fail()"));
            fail.Exception.Message.ShouldEqual("'Fail' failed!");

            failByAssertion.Name.ShouldEqual(TestClass + ".FailByAssertion");
            failByAssertion.Class.FullName.ShouldEqual(TestClass);
            failByAssertion.Method.Name.ShouldEqual("FailByAssertion");
            failByAssertion.Output.Lines().ShouldEqual("Console.Out: FailByAssertion", "Console.Error: FailByAssertion");
            failByAssertion.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
            failByAssertion.Exception.ShouldBeType<AssertActualExpectedException>();
            failByAssertion.Exception.CompoundStackTrace()
                .CleanStackTraceLineNumbers()
                .ShouldEqual(At("FailByAssertion()"));
            failByAssertion.Exception.Message.Lines().ShouldEqual(
                "Assertion Failure",
                "Expected: 2",
                "Actual:   1");

            skipWithReason.Name.ShouldEqual(TestClass + ".SkipWithReason");
            skipWithReason.Class.FullName.ShouldEqual(TestClass);
            skipWithReason.Method.Name.ShouldEqual("SkipWithReason");
            skipWithReason.Output.ShouldBeNull();
            skipWithReason.Duration.ShouldEqual(TimeSpan.Zero);
            skipWithReason.Reason.ShouldEqual("Skipped with reason.");

            skipWithoutReason.Name.ShouldEqual(TestClass + ".SkipWithoutReason");
            skipWithoutReason.Class.FullName.ShouldEqual(TestClass);
            skipWithoutReason.Method.Name.ShouldEqual("SkipWithoutReason");
            skipWithoutReason.Output.ShouldBeNull();
            skipWithoutReason.Duration.ShouldEqual(TimeSpan.Zero);
            skipWithoutReason.Reason.ShouldBeNull();

            var classCompleted = listener.ClassCompletions.Single();
            classCompleted.Class.FullName.ShouldEqual(FullName<MessagingTests>() + "+SampleTestClass");

            var assemblyCompleted = listener.AssemblyCompletions.Single();
            assemblyCompleted.Assembly.ShouldEqual(assembly);
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
