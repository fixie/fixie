using System;
using Should;

namespace Fixie.Tests.ClassFixtures
{
    public class DisposalTests
    {
        public void ShouldDisposeFixtureInstancesWhenDisposable()
        {
            var listener = new StubListener();

            DisposableFixture.ConstructionCount = 0;
            DisposableFixture.DisposalCount = 0;

            new SelfTestConvention().Execute(listener, typeof(DisposableFixture));

            listener.ShouldHaveEntries(
                "Fixie.Tests.ClassFixtures.DisposalTests+DisposableFixture.FailingCase failed: Failing Case",
                "Fixie.Tests.ClassFixtures.DisposalTests+DisposableFixture.PassingCase passed.");

            DisposableFixture.ConstructionCount.ShouldEqual(2);
            DisposableFixture.DisposalCount.ShouldEqual(2);
        }

        public void ShouldFailCasesWhenDisposeThrowsExceptionsWithoutSuppressingAnyExceptions()
        {
            var listener = new StubListener();

            new SelfTestConvention().Execute(listener, typeof(DisposeThrowsFixture));

            listener.ShouldHaveEntries(
                "Fixie.Tests.ClassFixtures.DisposalTests+DisposeThrowsFixture.FailingCase failed: Failing Case" + Environment.NewLine +
                "    Secondary Failure: Exception From IDisposable.Dispose().",
                "Fixie.Tests.ClassFixtures.DisposalTests+DisposeThrowsFixture.PassingCase failed: Exception From IDisposable.Dispose().");
        }

        class DisposableFixture : IDisposable
        {
            public static int ConstructionCount { get; set; }
            public static int DisposalCount { get; set; }
            bool disposed;

            public DisposableFixture()
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

        class DisposeThrowsFixture : IDisposable
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