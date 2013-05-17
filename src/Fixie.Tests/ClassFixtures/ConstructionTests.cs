using System;
using Should;

namespace Fixie.Tests.ClassFixtures
{
    public class ConstructionTests
    {
        public void ShouldConstructInstancePerCase()
        {
            var fixture = new ClassFixture();
            var listener = new StubListener();

            ConstructibleFixture.ConstructionCount = 0;

            fixture.Execute(typeof(ConstructibleFixture), new SelfTestConvention(), listener);

            ConstructibleFixture.ConstructionCount.ShouldEqual(2);
        }

        public void ShouldFailAllCasesWhenFixtureConstructorCannotBeInvoked()
        {
            var fixture = new ClassFixture();
            var listener = new StubListener();

            fixture.Execute(typeof(CannotInvokeConstructorFixture), new SelfTestConvention(), listener);

            listener.ShouldHaveEntries(
                "Fixie.Tests.ClassFixtures.ConstructionTests+CannotInvokeConstructorFixture.UnreachableCaseA failed: No parameterless constructor defined for this object.",
                "Fixie.Tests.ClassFixtures.ConstructionTests+CannotInvokeConstructorFixture.UnreachableCaseB failed: No parameterless constructor defined for this object.");
        }

        public void ShouldFailAllCasesWithOriginalExceptionWhenFixtureConstructorThrowsException()
        {
            var fixture = new ClassFixture();
            var listener = new StubListener();

            fixture.Execute(typeof(ConstructorThrowsFixture), new SelfTestConvention(), listener);

            listener.ShouldHaveEntries(
                "Fixie.Tests.ClassFixtures.ConstructionTests+ConstructorThrowsFixture.UnreachableCaseA failed: Exception From Constructor",
                "Fixie.Tests.ClassFixtures.ConstructionTests+ConstructorThrowsFixture.UnreachableCaseB failed: Exception From Constructor");
        }

        class ConstructibleFixture
        {
            public static int ConstructionCount { get; set; }

            public ConstructibleFixture()
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

        class CannotInvokeConstructorFixture
        {
            public CannotInvokeConstructorFixture(int argument)
            {
            }

            public void UnreachableCaseA() { }
            public void UnreachableCaseB() { }
        }

        class ConstructorThrowsFixture
        {
            public ConstructorThrowsFixture()
            {
                throw new Exception("Exception From Constructor");
            }

            public void UnreachableCaseA() { }
            public void UnreachableCaseB() { }
        }
    }
}