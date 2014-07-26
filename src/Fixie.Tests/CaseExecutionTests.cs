using System;
using Should;

namespace Fixie.Tests
{
    public class CaseExecutionTests
    {
        readonly Case @case;
        readonly CaseExecution execution;

        public CaseExecutionTests()
        {
            @case = new Case(typeof(SampleTestClass).GetInstanceMethod("Test"));
            execution = @case.Execution;
        }

        public void ShouldBeAssociatedWithCase()
        {
            execution.Case.ShouldEqual(@case);
        }

        public void ShouldTrackExceptionsAsFailureReasons()
        {
            var exceptionA = new InvalidOperationException();
            var exceptionB = new DivideByZeroException();

            execution.Exceptions.ShouldBeEmpty();
            execution.Fail(exceptionA);
            execution.Fail(exceptionB);
            execution.Exceptions.ShouldEqual(exceptionA, exceptionB);
        }

        public void CanSuppressFailuresByClearingExceptionLog()
        {
            var exceptionA = new InvalidOperationException();
            var exceptionB = new DivideByZeroException();

            execution.Exceptions.ShouldBeEmpty();
            execution.Fail(exceptionA);
            execution.Fail(exceptionB);
            execution.ClearExceptions();
            execution.Exceptions.ShouldBeEmpty();
        }

        private class SampleTestClass
        {
            public void Test() { }
        }
    }
}