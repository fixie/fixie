using System;
using Should;
using Xunit;

namespace Fixie.Tests
{
    public class ClassFixtureTests
    {
        readonly DefaultConvention defaultConvention;

        public ClassFixtureTests()
        {
            defaultConvention = new DefaultConvention();
        }

        [Fact]
        public void ShouldBeNamedAfterTheGivenFixtureClass()
        {
            var fixtureClass = typeof(ExecutionSampleFixture);
            var fixture = new ClassFixture(fixtureClass, defaultConvention);

            fixture.Name.ShouldEqual("Fixie.Tests.ClassFixtureTests+ExecutionSampleFixture");
        }

        [Fact]
        public void ShouldExecuteAllCases()
        {
            var listener = new StubListener();
            var fixtureClass = typeof(ExecutionSampleFixture);
            var fixture = new ClassFixture(fixtureClass, defaultConvention);

            fixture.Execute(listener);

            var result = listener.State.ToResult();
            result.Total.ShouldEqual(5);
            result.Passed.ShouldEqual(3);
            result.Failed.ShouldEqual(2);

            listener.Entries.ShouldEqual("Fixie.Tests.ClassFixtureTests+ExecutionSampleFixture.FailingCaseA failed: Failing Case A",
                                         "Fixie.Tests.ClassFixtureTests+ExecutionSampleFixture.PassingCaseA passed.",
                                         "Fixie.Tests.ClassFixtureTests+ExecutionSampleFixture.FailingCaseB failed: Failing Case B",
                                         "Fixie.Tests.ClassFixtureTests+ExecutionSampleFixture.PassingCaseB passed.",
                                         "Fixie.Tests.ClassFixtureTests+ExecutionSampleFixture.PassingCaseC passed.");
        }

        [Fact]
        public void ShouldFailAllCasesWhenFixtureConstructorCannotBeInvoked()
        {
            var listener = new StubListener();
            var fixtureClass = typeof(CannotInvokeConstructorSampleFixture);
            var fixture = new ClassFixture(fixtureClass, defaultConvention);

            fixture.Execute(listener);

            var result = listener.State.ToResult();
            result.Total.ShouldEqual(2);
            result.Passed.ShouldEqual(0);
            result.Failed.ShouldEqual(2);

            listener.Entries.ShouldEqual(
                "Fixie.Tests.ClassFixtureTests+CannotInvokeConstructorSampleFixture.UnreachableCaseA failed: No parameterless constructor defined for this object.",
                "Fixie.Tests.ClassFixtureTests+CannotInvokeConstructorSampleFixture.UnreachableCaseB failed: No parameterless constructor defined for this object.");
        }

        [Fact]
        public void ShouldFailAllCasesWithOriginalExceptionWhenFixtureConstructorThrowsException()
        {
            var listener = new StubListener();
            var fixtureClass = typeof(ConstructorThrowsSampleFixture);
            var fixture = new ClassFixture(fixtureClass, defaultConvention);

            fixture.Execute(listener);

            var result = listener.State.ToResult();
            result.Total.ShouldEqual(2);
            result.Passed.ShouldEqual(0);
            result.Failed.ShouldEqual(2);

            listener.Entries.ShouldEqual(
                "Fixie.Tests.ClassFixtureTests+ConstructorThrowsSampleFixture.UnreachableCaseA failed: Exception From Constructor",
                "Fixie.Tests.ClassFixtureTests+ConstructorThrowsSampleFixture.UnreachableCaseB failed: Exception From Constructor");
        }

        class ExecutionSampleFixture
        {
            public void FailingCaseA() { throw new Exception("Failing Case A"); }

            public void PassingCaseA() { }

            public void FailingCaseB() { throw new Exception("Failing Case B"); }

            public void PassingCaseB() { }

            public void PassingCaseC() { }
        }

        class CannotInvokeConstructorSampleFixture
        {
            public CannotInvokeConstructorSampleFixture(int argument)
            {
            }

            public void UnreachableCaseA() { }
            public void UnreachableCaseB() { }
        }

        class ConstructorThrowsSampleFixture
        {
            public ConstructorThrowsSampleFixture()
            {
                throw new Exception("Exception From Constructor");
            }

            public void UnreachableCaseA() { }
            public void UnreachableCaseB() { }
        }
    }
}