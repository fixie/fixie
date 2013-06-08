using System;
using Fixie.Conventions;
using Should;

namespace Fixie.Tests.ClassFixtures
{
    public class ConstructionTests
    {
        public void ShouldConstructInstancePerCase()
        {
            var listener = new StubListener();

            ConstructibleFixture.ConstructionCount = 0;

             new SelfTestConvention().Execute(listener, typeof(ConstructibleFixture));

            listener.ShouldHaveEntries(
                "Fixie.Tests.ClassFixtures.ConstructionTests+ConstructibleFixture.Fail failed: 'Fail' failed!",
                "Fixie.Tests.ClassFixtures.ConstructionTests+ConstructibleFixture.Pass passed.");

            ConstructibleFixture.ConstructionCount.ShouldEqual(2);
        }

        public void ShouldFailAllCasesWhenFixtureConstructorCannotBeInvoked()
        {
            var listener = new StubListener();

            new SelfTestConvention().Execute(listener, typeof(CannotInvokeConstructorFixture));

            listener.ShouldHaveEntries(
                "Fixie.Tests.ClassFixtures.ConstructionTests+CannotInvokeConstructorFixture.UnreachableCaseA failed: No parameterless constructor defined for this object.",
                "Fixie.Tests.ClassFixtures.ConstructionTests+CannotInvokeConstructorFixture.UnreachableCaseB failed: No parameterless constructor defined for this object.");
        }

        public void ShouldFailAllCasesWithOriginalExceptionWhenFixtureConstructorThrowsException()
        {
            var listener = new StubListener();

            new SelfTestConvention().Execute(listener, typeof(ConstructorThrowsFixture));

            listener.ShouldHaveEntries(
                "Fixie.Tests.ClassFixtures.ConstructionTests+ConstructorThrowsFixture.UnreachableCaseA failed: '.ctor' failed!",
                "Fixie.Tests.ClassFixtures.ConstructionTests+ConstructorThrowsFixture.UnreachableCaseB failed: '.ctor' failed!");
        }

        class ConstructibleFixture
        {
            public static int ConstructionCount { get; set; }

            public ConstructibleFixture()
            {
                ConstructionCount++;
            }

            public void Fail()
            {
                throw new FailureException();
            }

            public void Pass()
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
                throw new FailureException();
            }

            public void UnreachableCaseA() { }
            public void UnreachableCaseB() { }
        }
    }
}