using System;
using Fixie.Conventions;
using Should;

namespace Fixie.Tests.TestClasses
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
                "Fixie.Tests.TestClasses.DisposalTests+DisposableFixture.Fail failed: 'Fail' failed!",
                "Fixie.Tests.TestClasses.DisposalTests+DisposableFixture.Pass passed.");

            DisposableFixture.ConstructionCount.ShouldEqual(2);
            DisposableFixture.DisposalCount.ShouldEqual(2);
        }

        public void ShouldFailCasesWhenDisposeThrowsExceptionsWithoutSuppressingAnyExceptions()
        {
            var listener = new StubListener();

            new SelfTestConvention().Execute(listener, typeof(DisposeThrowsFixture));

            listener.ShouldHaveEntries(
                "Fixie.Tests.TestClasses.DisposalTests+DisposeThrowsFixture.Fail failed: 'Fail' failed!" + Environment.NewLine +
                "    Secondary Failure: 'Dispose' failed!",
                "Fixie.Tests.TestClasses.DisposalTests+DisposeThrowsFixture.Pass failed: 'Dispose' failed!");
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

            public void Fail()
            {
                throw new FailureException();
            }

            public void Pass() { }
        }

        class DisposeThrowsFixture : IDisposable
        {
            public void Dispose()
            {
                throw new FailureException();
            }

            public void Fail()
            {
                throw new FailureException();
            }

            public void Pass() { }
        }
    }
}