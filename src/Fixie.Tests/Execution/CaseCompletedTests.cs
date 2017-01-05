namespace Fixie.Tests.Execution
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;
    using System.Threading;
    using Fixie.Execution;
    using Fixie.Internal;
    using Should;
    using Should.Core.Exceptions;
    using static Utility;

    public class CaseCompletedTests
    {
        public void ShouldDescribeCaseCompletedMessages()
        {
            var convention = SelfTestConvention.Build();
            convention.CaseExecution.Skip(x => x.Method.Has<SkipAttribute>(), x => x.Method.GetCustomAttribute<SkipAttribute>().Reason);
            convention.HideExceptionDetails.For<EqualException>();

            var listener = new StubCaseCompletedListener();

            using (new RedirectedConsole())
            {
                typeof(SampleTestClass).Run(listener, convention);

                listener.Log.Count.ShouldEqual(5);

                var skipWithReason = (CaseSkipped)listener.Log[0];
                var skipWithoutReason = (CaseSkipped)listener.Log[1];
                var fail = (CaseFailed)listener.Log[2];
                var failByAssertion = (CaseFailed)listener.Log[3];
                var pass = listener.Log[4];

                pass.Name.ShouldEqual(FullName<SampleTestClass>() + ".Pass");
                pass.Class.FullName.ShouldEqual(FullName<SampleTestClass>() + "");
                pass.Method.Name.ShouldEqual("Pass");
                pass.Output.Lines().ShouldEqual("Console.Out: Pass", "Console.Error: Pass");
                pass.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
                pass.Status.ShouldEqual(CaseStatus.Passed);

                fail.Name.ShouldEqual(FullName<SampleTestClass>() + ".Fail");
                fail.Class.FullName.ShouldEqual(FullName<SampleTestClass>() + "");
                fail.Method.Name.ShouldEqual("Fail");
                fail.Output.Lines().ShouldEqual("Console.Out: Fail", "Console.Error: Fail");
                fail.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
                fail.Status.ShouldEqual(CaseStatus.Failed);
                fail.Exception.Type.ShouldEqual("Fixie.Tests.FailureException");
                CleanBrittleValues(fail.Exception.StackTrace)
                   .Lines()
                   .ShouldEqual("'Fail' failed!", At<SampleTestClass>("Fail()"));
                fail.Exception.Message.ShouldEqual("'Fail' failed!");

                failByAssertion.Name.ShouldEqual(FullName<SampleTestClass>() + ".FailByAssertion");
                failByAssertion.Class.FullName.ShouldEqual(FullName<SampleTestClass>() + "");
                failByAssertion.Method.Name.ShouldEqual("FailByAssertion");
                failByAssertion.Output.Lines().ShouldEqual("Console.Out: FailByAssertion", "Console.Error: FailByAssertion");
                failByAssertion.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
                failByAssertion.Status.ShouldEqual(CaseStatus.Failed);
                failByAssertion.Exception.Type.ShouldEqual("Should.Core.Exceptions.EqualException");
                CleanBrittleValues(failByAssertion.Exception.StackTrace)
                    .Lines()
                    .ShouldEqual(
                        "Assert.Equal() Failure",
                        "Expected: 2",
                        "Actual:   1",
                        At<SampleTestClass>("FailByAssertion()"));
                failByAssertion.Exception.Message.Lines().ShouldEqual(
                    "Assert.Equal() Failure",
                    "Expected: 2",
                    "Actual:   1");

                skipWithReason.Name.ShouldEqual(FullName<SampleTestClass>() + ".SkipWithReason");
                skipWithReason.Class.FullName.ShouldEqual(FullName<SampleTestClass>() + "");
                skipWithReason.Method.Name.ShouldEqual("SkipWithReason");
                skipWithReason.Output.ShouldBeNull();
                skipWithReason.Duration.ShouldEqual(TimeSpan.Zero);
                skipWithReason.Status.ShouldEqual(CaseStatus.Skipped);
                skipWithReason.Reason.ShouldEqual("Skipped with reason.");

                skipWithoutReason.Name.ShouldEqual(FullName<SampleTestClass>() + ".SkipWithoutReason");
                skipWithoutReason.Class.FullName.ShouldEqual(FullName<SampleTestClass>() + "");
                skipWithoutReason.Method.Name.ShouldEqual("SkipWithoutReason");
                skipWithoutReason.Output.ShouldBeNull();
                skipWithoutReason.Duration.ShouldEqual(TimeSpan.Zero);
                skipWithoutReason.Status.ShouldEqual(CaseStatus.Skipped);
                skipWithoutReason.Reason.ShouldBeNull();
            }
        }

        public class StubCaseCompletedListener :
            Handler<CaseSkipped>,
            Handler<CasePassed>,
            Handler<CaseFailed>
        {
            public List<CaseCompleted> Log { get; set; } = new List<CaseCompleted>();

            public void Handle(CaseSkipped message) => Log.Add(message);
            public void Handle(CasePassed message) => Log.Add(message);
            public void Handle(CaseFailed message) => Log.Add(message);
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
