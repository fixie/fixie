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

                var skip = (CaseSkipped)listener.Log[0];
                var skipWithReason = (CaseSkipped)listener.Log[1];
                var fail = (CaseFailed)listener.Log[2];
                var failByAssertion = (CaseFailed)listener.Log[3];
                var pass = listener.Log[4];

                pass.Name.ShouldEqual("Fixie.Tests.Execution.CaseCompletedTests+SampleTestClass.Pass");
                pass.Class.FullName.ShouldEqual("Fixie.Tests.Execution.CaseCompletedTests+SampleTestClass");
                pass.Method.Name.ShouldEqual("Pass");
                pass.Output.ShouldEqual("Pass" + Environment.NewLine);
                pass.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
                pass.Status.ShouldEqual(CaseStatus.Passed);

                fail.Name.ShouldEqual("Fixie.Tests.Execution.CaseCompletedTests+SampleTestClass.Fail");
                fail.Class.FullName.ShouldEqual("Fixie.Tests.Execution.CaseCompletedTests+SampleTestClass");
                fail.Method.Name.ShouldEqual("Fail");
                fail.Output.ShouldEqual("Fail" + Environment.NewLine);
                fail.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
                fail.Status.ShouldEqual(CaseStatus.Failed);
                fail.Exception.PrimaryException.Type.ShouldEqual("Fixie.Tests.FailureException");
                fail.Exception.CompoundStackTrace.ShouldNotBeNull();
                fail.Exception.Message.ShouldEqual("'Fail' failed!");

                failByAssertion.Name.ShouldEqual("Fixie.Tests.Execution.CaseCompletedTests+SampleTestClass.FailByAssertion");
                failByAssertion.Class.FullName.ShouldEqual("Fixie.Tests.Execution.CaseCompletedTests+SampleTestClass");
                failByAssertion.Method.Name.ShouldEqual("FailByAssertion");
                failByAssertion.Output.ShouldEqual("FailByAssertion" + Environment.NewLine);
                failByAssertion.Duration.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
                failByAssertion.Status.ShouldEqual(CaseStatus.Failed);
                failByAssertion.Exception.PrimaryException.Type.ShouldEqual("Should.Core.Exceptions.EqualException");
                failByAssertion.Exception.CompoundStackTrace.ShouldNotBeNull();
                failByAssertion.Exception.Message.Lines().ShouldEqual(
                    "Assert.Equal() Failure",
                    "Expected: 2",
                    "Actual:   1");

                skip.Name.ShouldEqual("Fixie.Tests.Execution.CaseCompletedTests+SampleTestClass.Skip");
                skip.Class.FullName.ShouldEqual("Fixie.Tests.Execution.CaseCompletedTests+SampleTestClass");
                skip.Method.Name.ShouldEqual("Skip");
                skip.Output.ShouldBeNull();
                skip.Duration.ShouldEqual(TimeSpan.Zero);
                skip.Status.ShouldEqual(CaseStatus.Skipped);
                skip.Reason.ShouldBeNull();

                skipWithReason.Name.ShouldEqual("Fixie.Tests.Execution.CaseCompletedTests+SampleTestClass.SkipWithReason");
                skipWithReason.Class.FullName.ShouldEqual("Fixie.Tests.Execution.CaseCompletedTests+SampleTestClass");
                skipWithReason.Method.Name.ShouldEqual("SkipWithReason");
                skipWithReason.Output.ShouldBeNull();
                skipWithReason.Duration.ShouldEqual(TimeSpan.Zero);
                skipWithReason.Status.ShouldEqual(CaseStatus.Skipped);
                skipWithReason.Reason.ShouldEqual("Skipped by naming convention.");
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
