using System;
using System.Reflection;
using Should;

namespace Fixie.Tests
{
    public class CaseResultTests
    {
        readonly Case @case;
        readonly CaseResult result;

        public CaseResultTests()
        {
            @case = Case<SampleTestClass>("Test");
            result = new CaseResult(@case);
        }

        public void ShouldBeAssociatedWithCase()
        {
            result.Case.ShouldEqual(@case);
        }

        public void ShouldTrackExceptionsAsFailureReasons()
        {
            var exceptionA = new InvalidOperationException();
            var exceptionB = new DivideByZeroException();

            result.Exceptions.ShouldBeEmpty();
            result.Fail(exceptionA);
            result.Fail(exceptionB);
            result.Exceptions.ShouldEqual(exceptionA, exceptionB);
        }

        public void ShouldDetermineCaseStatus()
        {
            result.Status.ShouldEqual(CaseStatus.Passed);

            result.Fail(new Exception());

            result.Status.ShouldEqual(CaseStatus.Failed);
        }

        private class SampleTestClass
        {
            public void Test() { }
        }

        static Case Case<TTestClass>(string method)
        {
            return new Case(typeof(TTestClass), typeof(TTestClass).GetMethod(method, BindingFlags.Instance | BindingFlags.Public));
        }
    }
}