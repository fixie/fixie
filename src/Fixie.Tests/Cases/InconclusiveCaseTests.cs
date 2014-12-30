using System;
using System.Linq;

namespace Fixie.Tests.Cases
{
    public class InconclusiveCaseTests : CaseTests
    {
        public void ShouldAllowMarkingCasesWithExceptionsAsInconclusive()
        {
            Convention.CaseExecution
                .Wrap<TreatNotImplementedAsInconclusivePreservingException>();

            Run<SampleTestClass>();

            Listener.Entries.ShouldEqual(
                "Fixie.Tests.Cases.InconclusiveCaseTests+SampleTestClass.Fail failed: 'Fail' failed!",
                "Fixie.Tests.Cases.InconclusiveCaseTests+SampleTestClass.Inconclusive was inconclusive: The method or operation is not implemented.",
                "Fixie.Tests.Cases.InconclusiveCaseTests+SampleTestClass.Pass passed.");
        }

        public void ShouldAllowMarkingCasesWithoutExceptionsAsInconclusive()
        {
            Convention.CaseExecution
                .Wrap<TreatNotImplementedAsInconclusiveClearingException>();

            Run<SampleTestClass>();

            Listener.Entries.ShouldEqual(
                "Fixie.Tests.Cases.InconclusiveCaseTests+SampleTestClass.Fail failed: 'Fail' failed!",
                "Fixie.Tests.Cases.InconclusiveCaseTests+SampleTestClass.Inconclusive was inconclusive.",
                "Fixie.Tests.Cases.InconclusiveCaseTests+SampleTestClass.Pass passed.");
        }

        class SampleTestClass
        {
            public void Pass() { }
            public void Fail() { throw new FailureException(); }
            public void Inconclusive() { throw new NotImplementedException(); }
        }

        class TreatNotImplementedAsInconclusivePreservingException : CaseBehavior
        {
            public void Execute(Case @case, Action next)
            {
                next();

                if (@case.Exceptions.Any() && @case.Exceptions.First() is NotImplementedException)
                {
                    @case.Inconclusive = true;
                }
            }
        }

        class TreatNotImplementedAsInconclusiveClearingException : CaseBehavior
        {
            public void Execute(Case @case, Action next)
            {
                next();

                if (@case.Exceptions.Any() && @case.Exceptions.First() is NotImplementedException)
                {
                    @case.Inconclusive = true;
                    @case.ClearExceptions();
                }
            }
        }
    }
}