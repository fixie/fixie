using System;
using Should;

namespace Fixie.Tests.ClassFixtures
{
    public class DisposalTests
    {
        public void ShouldDisposeFixtureInstancesWhenDisposable()
        {
            var listener = new StubListener();
            var defaultConvention = new DefaultConvention();
            var fixtureClass = typeof(DisposableSampleFixture);
            var fixture = new ClassFixture(fixtureClass, defaultConvention);

            DisposableSampleFixture.ConstructionCount = 0;
            DisposableSampleFixture.DisposalCount = 0;

            fixture.Execute(listener);

            listener.ShouldHaveEntries(
                "Fixie.Tests.ClassFixtures.DisposalTests+DisposableSampleFixture.FailingCase failed: Failing Case",
                "Fixie.Tests.ClassFixtures.DisposalTests+DisposableSampleFixture.PassingCase passed.");

            DisposableSampleFixture.ConstructionCount.ShouldEqual(2);
            DisposableSampleFixture.DisposalCount.ShouldEqual(2);
        }

        public void ShouldFailCasesWhenDisposeThrowsExceptionsWithoutSuppressingAnyExceptions()
        {
            var listener = new StubListener();
            var defaultConvention = new DefaultConvention();
            var fixtureClass = typeof(DisposeThrowsSampleFixture);
            var fixture = new ClassFixture(fixtureClass, defaultConvention);

            fixture.Execute(listener);

            listener.ShouldHaveEntries(
                "Fixie.Tests.ClassFixtures.DisposalTests+DisposeThrowsSampleFixture.FailingCase failed: Failing Case" + Environment.NewLine +
                "    Secondary Failure: Exception From IDisposable.Dispose().",
                "Fixie.Tests.ClassFixtures.DisposalTests+DisposeThrowsSampleFixture.PassingCase failed: Exception From IDisposable.Dispose().");
        }

        class DisposableSampleFixture : IDisposable
        {
            public static int ConstructionCount { get; set; }
            public static int DisposalCount { get; set; }
            bool disposed;

            public DisposableSampleFixture()
            {
                ConstructionCount++;
            }

            public void Dispose()
            {
                if (disposed)
                    throw new ShouldBeUnreachableException();

                DisposalCount++;
                disposed = true;
            }

            public void FailingCase()
            {
                throw new Exception("Failing Case");
            }

            public void PassingCase() { }
        }

        class DisposeThrowsSampleFixture : IDisposable
        {
            public void Dispose()
            {
                throw new Exception("Exception From IDisposable.Dispose().");
            }

            public void FailingCase()
            {
                throw new Exception("Failing Case");
            }

            public void PassingCase() { }
        }
    }
}