using System;
using Should;

namespace Fixie.Tests
{
    public class CaseExecutionTests
    {
        readonly CaseExecution execution;

        public CaseExecutionTests()
        {
            execution = new CaseExecution();
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
    }
}