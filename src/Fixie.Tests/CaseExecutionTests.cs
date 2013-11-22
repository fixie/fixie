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
            @case = Case<SampleTestClass>("Test");
            execution = new CaseExecution(@case);
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

        public void ShouldDetermineCaseResult()
        {
            execution.Result.ShouldEqual(CaseResult.Passed);

            execution.Fail(new Exception());

            execution.Result.ShouldEqual(CaseResult.Failed);
        }

        private class SampleTestClass
        {
            public void Test() { }
        }

        static Case Case<TTestClass>(string method)
        {
            return new Case(typeof(TTestClass), typeof(TTestClass).GetInstanceMethod(method));
        }
    }
}