using System;
using System.Reflection;
using Should;
using Xunit;

namespace Fixie.Tests
{
    public class MethodCaseTests
    {
        readonly Type fixtureClass;
        readonly MethodInfo passingMethod;
        readonly MethodInfo failingMethod;

        public MethodCaseTests()
        {
            fixtureClass = typeof(SampleFixture);
            passingMethod = fixtureClass.GetMethod("Pass", BindingFlags.Public | BindingFlags.Instance);
            failingMethod = fixtureClass.GetMethod("Fail", BindingFlags.Public | BindingFlags.Instance);
        }

        [Fact]
        public void ShouldBeNamedAfterTheGivenMethod()
        {
            var passingCase = new MethodCase(fixtureClass, passingMethod);
            var failingCase = new MethodCase(fixtureClass, failingMethod);

            passingCase.Name.ShouldEqual("Fixie.Tests.MethodCaseTests+SampleFixture.Pass");
            failingCase.Name.ShouldEqual("Fixie.Tests.MethodCaseTests+SampleFixture.Fail");
        }

        [Fact]
        public void ShouldInvokeTheGivenMethodWhenExecuted()
        {
            var passingCase = new MethodCase(fixtureClass, passingMethod);

            SampleFixture.MethodInvoked = false;
            passingCase.Execute();
            SampleFixture.MethodInvoked.ShouldBeTrue();
        }

        [Fact]
        public void ShouldReportPassingResultUponSuccessfulExecution()
        {
            var passingCase = new MethodCase(fixtureClass, passingMethod);

            var result = passingCase.Execute();

            result.Passed.ShouldBeTrue();
            result.Exception.ShouldBeNull();
        }

        [Fact]
        public void ShouldReportFailingResultWithOriginalExceptionUponUnsuccessfulExecution()
        {
            var failingCase = new MethodCase(fixtureClass, failingMethod);

            var result = failingCase.Execute();

            result.Passed.ShouldBeFalse();
            result.Exception.ShouldBeType<MethodInvokedException>();
        }

        class MethodInvokedException : Exception { }

        class SampleFixture
        {
            public static bool MethodInvoked;

            public void Pass()
            {
                MethodInvoked = true;
            }

            public void Fail()
            {
                throw new MethodInvokedException();
            }
        }
    }
}