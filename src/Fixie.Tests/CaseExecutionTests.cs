using System;
using Should;

namespace Fixie.Tests
{
    public class CaseExecutionTests
    {
        readonly Case @case;

        public CaseExecutionTests()
        {
            @case = new Case(typeof(SampleTestClass).GetInstanceMethod("TestMethod"));
        }

        public void ShouldTrackExceptionsAsFailureReasons()
        {
            var exceptionA = new InvalidOperationException();
            var exceptionB = new DivideByZeroException();

            @case.Exceptions.ShouldBeEmpty();
            @case.Fail(exceptionA);
            @case.Fail(exceptionB);
            @case.Exceptions.ShouldEqual(exceptionA, exceptionB);
        }

        public void CanSuppressFailuresByClearingExceptionLog()
        {
            var exceptionA = new InvalidOperationException();
            var exceptionB = new DivideByZeroException();

            @case.Exceptions.ShouldBeEmpty();
            @case.Fail(exceptionA);
            @case.Fail(exceptionB);
            @case.ClearExceptions();
            @case.Exceptions.ShouldBeEmpty();
        }

        class SampleTestClass
        {
            public void TestMethod()
            {
            }
        }
    }
}