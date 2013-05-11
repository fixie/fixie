using System;
using Should;

namespace Fixie.Tests.ClassFixtures
{
    public class ConstructionTests
    {
        public void ShouldConstructInstancePerCase()
        {
            var listener = new StubListener();
            var defaultConvention = new DefaultConvention();
            var fixtureClass = typeof(ConstructibleSampleFixture);
            var fixture = new ClassFixture(fixtureClass, defaultConvention);

            ConstructibleSampleFixture.ConstructionCount = 0;

            fixture.Execute(listener);

            ConstructibleSampleFixture.ConstructionCount.ShouldEqual(2);
        }

        public void ShouldFailAllCasesWhenFixtureConstructorCannotBeInvoked()
        {
            var listener = new StubListener();
            var defaultConvention = new DefaultConvention();
            var fixtureClass = typeof(CannotInvokeConstructorSampleFixture);
            var fixture = new ClassFixture(fixtureClass, defaultConvention);

            fixture.Execute(listener);

            listener.ShouldHaveEntries(
                "Fixie.Tests.ClassFixtures.ConstructionTests+CannotInvokeConstructorSampleFixture.UnreachableCaseA failed: No parameterless constructor defined for this object.",
                "Fixie.Tests.ClassFixtures.ConstructionTests+CannotInvokeConstructorSampleFixture.UnreachableCaseB failed: No parameterless constructor defined for this object.");
        }

        public void ShouldFailAllCasesWithOriginalExceptionWhenFixtureConstructorThrowsException()
        {
            var listener = new StubListener();
            var defaultConvention = new DefaultConvention();
            var fixtureClass = typeof(ConstructorThrowsSampleFixture);
            var fixture = new ClassFixture(fixtureClass, defaultConvention);

            fixture.Execute(listener);

            listener.ShouldHaveEntries(
                "Fixie.Tests.ClassFixtures.ConstructionTests+ConstructorThrowsSampleFixture.UnreachableCaseA failed: Exception From Constructor",
                "Fixie.Tests.ClassFixtures.ConstructionTests+ConstructorThrowsSampleFixture.UnreachableCaseB failed: Exception From Constructor");
        }

        class ConstructibleSampleFixture
        {
            public static int ConstructionCount { get; set; }

            public ConstructibleSampleFixture()
            {
                ConstructionCount++;
            }

            public void FailingCase()
            {
                throw new Exception("Failing Case");
            }

            public void PassingCase()
            {
            }
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