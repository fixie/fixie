namespace Fixie.Tests.Execution
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using Fixie.Execution;
    using Fixie.Internal;
    using Should;
    using Should.Core.Exceptions;

    public class CaseCompletedTests
    {
        public void ShouldDescribeCaseCompletedMessages()
        {
            var convention = SelfTestConvention.Build();
            convention.CaseExecution.Skip(x => x.Method.Name == "Skip");
            convention.CaseExecution.Skip(x => x.Method.Name == "SkipWithReason", x => "Skipped by naming convention.");
            convention.HideExceptionDetails.For<EqualException>();

            var listener = new StubCaseCompletedListener();

            using (new RedirectedConsole())
            {
                typeof(SampleTestClass).Run(listener, convention);

                listener.Log.Count.ShouldEqual(5);

                var skip = listener.Log[0];
                var skipWithReason = listener.Log[1];
                var fail = listener.Log[2];
                var failByAssertion = listener.Log[3];
                var pass = listener.Log[4];

                pass.Name.ShouldEqual("Fixie.Tests.Execution.CaseCompletedTests+SampleTestClass.Pass");
                pass.MethodGroup.FullName.ShouldEqual("Fixie.Tests.Execution.CaseCompletedTests+SampleTestClass.Pass");
                pass.Output.ShouldEqual("Pass" + Environment.NewLine);
                pass.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
                pass.Status.ShouldEqual(CaseStatus.Passed);
                pass.Exceptions.ShouldBeNull();
                pass.SkipReason.ShouldBeNull();

                fail.Name.ShouldEqual("Fixie.Tests.Execution.CaseCompletedTests+SampleTestClass.Fail");
                fail.MethodGroup.FullName.ShouldEqual("Fixie.Tests.Execution.CaseCompletedTests+SampleTestClass.Fail");
                fail.Output.ShouldEqual("Fail" + Environment.NewLine);
                fail.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
                fail.Status.ShouldEqual(CaseStatus.Failed);
                fail.Exceptions.PrimaryException.Type.ShouldEqual("Fixie.Tests.FailureException");
                fail.Exceptions.CompoundStackTrace.ShouldNotBeNull();
                fail.Exceptions.PrimaryException.Message.ShouldEqual("'Fail' failed!");
                fail.SkipReason.ShouldBeNull();

                failByAssertion.Name.ShouldEqual("Fixie.Tests.Execution.CaseCompletedTests+SampleTestClass.FailByAssertion");
                failByAssertion.MethodGroup.FullName.ShouldEqual("Fixie.Tests.Execution.CaseCompletedTests+SampleTestClass.FailByAssertion");
                failByAssertion.Output.ShouldEqual("FailByAssertion" + Environment.NewLine);
                failByAssertion.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
                failByAssertion.Status.ShouldEqual(CaseStatus.Failed);
                failByAssertion.Exceptions.PrimaryException.Type.ShouldEqual("Should.Core.Exceptions.EqualException");
                failByAssertion.Exceptions.CompoundStackTrace.ShouldNotBeNull();
                failByAssertion.Exceptions.PrimaryException.Message.Lines().ShouldEqual(
                    "Assert.Equal() Failure",
                    "Expected: 2",
                    "Actual:   1");
                failByAssertion.SkipReason.ShouldBeNull();

                skip.Name.ShouldEqual("Fixie.Tests.Execution.CaseCompletedTests+SampleTestClass.Skip");
                skip.MethodGroup.FullName.ShouldEqual("Fixie.Tests.Execution.CaseCompletedTests+SampleTestClass.Skip");
                skip.Output.ShouldBeNull();
                skip.Duration.ShouldEqual(TimeSpan.Zero);
                skip.Status.ShouldEqual(CaseStatus.Skipped);
                skip.Exceptions.ShouldBeNull();
                skip.SkipReason.ShouldBeNull();

                skipWithReason.Name.ShouldEqual("Fixie.Tests.Execution.CaseCompletedTests+SampleTestClass.SkipWithReason");
                skipWithReason.MethodGroup.FullName.ShouldEqual("Fixie.Tests.Execution.CaseCompletedTests+SampleTestClass.SkipWithReason");
                skipWithReason.Output.ShouldBeNull();
                skipWithReason.Duration.ShouldEqual(TimeSpan.Zero);
                skipWithReason.Status.ShouldEqual(CaseStatus.Skipped);
                skipWithReason.Exceptions.ShouldBeNull();
                skipWithReason.SkipReason.ShouldEqual("Skipped by naming convention.");
            }
        }

        public class StubCaseCompletedListener : Listener
        {
            public List<CaseCompleted> Log { get; set; } = new List<CaseCompleted>();

            public void AssemblyStarted(AssemblyInfo message) { }

            public void CaseSkipped(CaseSkipped message) => Log.Add(message);
            public void CasePassed(CasePassed message) => Log.Add(message);
            public void CaseFailed(CaseFailed message) => Log.Add(message);

            public void AssemblyCompleted(AssemblyInfo message, AssemblyResult result) { }
        }

        static void WhereAmI([CallerMemberName] string member = null)
        {
            Console.WriteLine(member);
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

            public void Skip()
            {
                WhereAmI();
            }

            public void SkipWithReason()
            {
                WhereAmI();
            }
        }
    }
}
