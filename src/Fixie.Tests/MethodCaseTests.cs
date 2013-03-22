using System;
using System.Reflection;
using Should;
using Xunit;

namespace Fixie.Tests
{
    public class MethodCaseTests
    {
        readonly StubListener listener;
        readonly Type fixtureClass;
        readonly ClassFixture fixture;
        readonly MethodInfo passingMethod;
        readonly MethodInfo failingMethod;
        readonly MethodInfo cannotInvokeMethod;

        public MethodCaseTests()
        {
            listener = new StubListener();
            fixtureClass = typeof(SampleFixture);
            fixture = new ClassFixture(fixtureClass, new SampleFixture());
            passingMethod = fixtureClass.GetMethod("Pass", BindingFlags.Public | BindingFlags.Instance);
            failingMethod = fixtureClass.GetMethod("Fail", BindingFlags.Public | BindingFlags.Instance);
            cannotInvokeMethod = fixtureClass.GetMethod("CannotInvoke", BindingFlags.Public | BindingFlags.Instance);
        }

        [Fact]
        public void ShouldBeNamedAfterTheGivenMethod()
        {
            var passingCase = new MethodCase(fixture, passingMethod);
            var failingCase = new MethodCase(fixture, failingMethod);

            passingCase.Name.ShouldEqual("Fixie.Tests.MethodCaseTests+SampleFixture.Pass");
            failingCase.Name.ShouldEqual("Fixie.Tests.MethodCaseTests+SampleFixture.Fail");
        }

        [Fact]
        public void ShouldInvokeTheGivenMethodWhenExecuted()
        {
            var passingCase = new MethodCase(fixture, passingMethod);

            SampleFixture.MethodInvoked = false;
            passingCase.Execute(listener);
            SampleFixture.MethodInvoked.ShouldBeTrue();
        }

        [Fact]
        public void ShouldReportPassingResultUponSuccessfulExecution()
        {
            var passingCase = new MethodCase(fixture, passingMethod);

            var result = passingCase.Execute(listener);

            result.ShouldEqual(Result.Pass);
        }

        [Fact]
        public void ShouldReportFailingResultWhenCaseMethodCannotBeInvoked()
        {
            var cannotInvokeCase = new MethodCase(fixture, cannotInvokeMethod);

            var result = cannotInvokeCase.Execute(listener);

            result.ShouldEqual(Result.Fail);
            listener.Entries.ShouldEqual("Fixie.Tests.MethodCaseTests+SampleFixture.CannotInvoke failed: Parameter count mismatch.");
        }

        [Fact]
        public void ShouldReportFailingResultWithOriginalExceptionWhenCaseMethodThrowsException()
        {
            var failingCase = new MethodCase(fixture, failingMethod);

            var result = failingCase.Execute(listener);

            result.ShouldEqual(Result.Fail);
            listener.Entries.ShouldEqual("Fixie.Tests.MethodCaseTests+SampleFixture.Fail failed: Exception of type " +
                                         "'Fixie.Tests.MethodCaseTests+MethodInvokedException' was thrown.");
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

            public void CannotInvoke(int argument)
            {
            }
        }
    }
}