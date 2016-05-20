namespace Fixie.Tests.Execution
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;
    using System.Threading;
    using Fixie.Execution;
    using Fixie.Internal;
    using Should;
    using Should.Core.Exceptions;
    using static Utility;

    public class LifecycleMessageTests
    {
        public void ShouldDescribeCaseCompletion()
        {
            var convention = SelfTestConvention.Build();
            convention.CaseExecution.Skip(x => x.Method.Has<SkipAttribute>(), x => x.Method.GetCustomAttribute<SkipAttribute>().Reason);
            convention.HideExceptionDetails.For<EqualException>();

            var listener = new StubCaseCompletedListener();

            using (new RedirectedConsole())
            {
                typeof(SampleTestClass).Run(listener, convention);

                var assembly = typeof(LifecycleMessageTests).Assembly;

                var assemblyStarted = listener.AssemblyStarts.Single();
                assemblyStarted.Name.ShouldEqual("Fixie.Tests");
                assemblyStarted.Location.ShouldEqual(assembly.Location);

                listener.Cases.Count.ShouldEqual(5);

                var skipWithReason = (CaseSkipped)listener.Cases[0];
                var skipWithoutReason = (CaseSkipped)listener.Cases[1];
                var fail = (CaseFailed)listener.Cases[2];
                var failByAssertion = (CaseFailed)listener.Cases[3];
                var pass = listener.Cases[4];

                pass.Name.ShouldEqual(FullName<SampleTestClass>() + ".Pass");
                pass.MethodGroup.FullName.ShouldEqual(FullName<SampleTestClass>() + ".Pass");
                pass.Output.Lines().ShouldEqual("Console.Out: Pass", "Console.Error: Pass");
                pass.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
                pass.Status.ShouldEqual(CaseStatus.Passed);

                fail.Name.ShouldEqual(FullName<SampleTestClass>() + ".Fail");
                fail.MethodGroup.FullName.ShouldEqual(FullName<SampleTestClass>() + ".Fail");
                fail.Output.Lines().ShouldEqual("Console.Out: Fail", "Console.Error: Fail");
                fail.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
                fail.Status.ShouldEqual(CaseStatus.Failed);
                fail.Exception.FailedAssertion.ShouldBeFalse();
                fail.Exception.Type.ShouldEqual("Fixie.Tests.FailureException");
                CleanBrittleValues(fail.Exception.StackTrace)
                    .ShouldEqual(At<SampleTestClass>("Fail()"));
                fail.Exception.Message.ShouldEqual("'Fail' failed!");

                failByAssertion.Name.ShouldEqual(FullName<SampleTestClass>() + ".FailByAssertion");
                failByAssertion.MethodGroup.FullName.ShouldEqual(FullName<SampleTestClass>() + ".FailByAssertion");
                failByAssertion.Output.Lines().ShouldEqual("Console.Out: FailByAssertion", "Console.Error: FailByAssertion");
                failByAssertion.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
                failByAssertion.Status.ShouldEqual(CaseStatus.Failed);
                failByAssertion.Exception.FailedAssertion.ShouldBeTrue();
                failByAssertion.Exception.Type.ShouldEqual("Should.Core.Exceptions.EqualException");
                CleanBrittleValues(failByAssertion.Exception.StackTrace)
                    .ShouldEqual(At<SampleTestClass>("FailByAssertion()"));
                failByAssertion.Exception.Message.Lines().ShouldEqual(
                    "Assert.Equal() Failure",
                    "Expected: 2",
                    "Actual:   1");

                skipWithReason.Name.ShouldEqual(FullName<SampleTestClass>() + ".SkipWithReason");
                skipWithReason.MethodGroup.FullName.ShouldEqual(FullName<SampleTestClass>() + ".SkipWithReason");
                skipWithReason.Output.ShouldBeNull();
                skipWithReason.Duration.ShouldEqual(TimeSpan.Zero);
                skipWithReason.Status.ShouldEqual(CaseStatus.Skipped);
                skipWithReason.Reason.ShouldEqual("Skipped with reason.");

                skipWithoutReason.Name.ShouldEqual(FullName<SampleTestClass>() + ".SkipWithoutReason");
                skipWithoutReason.MethodGroup.FullName.ShouldEqual(FullName<SampleTestClass>() + ".SkipWithoutReason");
                skipWithoutReason.Output.ShouldBeNull();
                skipWithoutReason.Duration.ShouldEqual(TimeSpan.Zero);
                skipWithoutReason.Status.ShouldEqual(CaseStatus.Skipped);
                skipWithoutReason.Reason.ShouldBeNull();

                var assemblyCompleted = listener.AssemblyCompletions.Single();
                assemblyCompleted.Name.ShouldEqual("Fixie.Tests");
                assemblyCompleted.Location.ShouldEqual(assembly.Location);
            }
        }

        public class StubCaseCompletedListener :
            Handler<AssemblyStarted>,
            Handler<CaseCompleted>,
            Handler<AssemblyCompleted>
        {
            public List<AssemblyStarted> AssemblyStarts { get; set; } = new List<AssemblyStarted>();
            public List<CaseCompleted> Cases { get; set; } = new List<CaseCompleted>();
            public List<AssemblyCompleted> AssemblyCompletions { get; set; } = new List<AssemblyCompleted>();

            public void Handle(AssemblyStarted message) => AssemblyStarts.Add(message);
            public void Handle(CaseCompleted message) => Cases.Add(message);
            public void Handle(AssemblyCompleted message) => AssemblyCompletions.Add(message);
        }

        static string CleanBrittleValues(string actualRawContent)
        {
            //Avoid brittle assertion introduced by test duration.
            var decimalSeparator = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            var cleaned = Regex.Replace(actualRawContent, @"took [\d" + Regex.Escape(decimalSeparator) + @"]+ seconds", @"took 1.23 seconds");

            //Avoid brittle assertion introduced by stack trace line numbers.
            cleaned = Regex.Replace(cleaned, @":line \d+", ":line #");

            return cleaned;
        }

        class SampleTestClass
        {
            public void Fail()
            {
                WhereAmI();
                throw new FailureException();
            }

            public void FailByAssertion()
            {
                WhereAmI();
                1.ShouldEqual(2);
            }

            public void Pass()
            {
                WhereAmI();
            }

            [Skip]
            public void SkipWithoutReason() { throw new ShouldBeUnreachableException(); }

            [Skip("Skipped with reason.")]
            public void SkipWithReason() { throw new ShouldBeUnreachableException(); }

            static void WhereAmI([CallerMemberName] string member = null)
            {
                Console.Out.WriteLine("Console.Out: " + member);
                Console.Error.WriteLine("Console.Error: " + member);
            }
        }
    }
}
