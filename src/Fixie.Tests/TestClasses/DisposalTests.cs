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

            DisposableTestClass.ConstructionCount = 0;
            DisposableTestClass.DisposalCount = 0;

            new SelfTestConvention().Execute(listener, typeof(DisposableTestClass));

            listener.ShouldHaveEntries(
                "Fixie.Tests.TestClasses.DisposalTests+DisposableTestClass.Fail failed: 'Fail' failed!",
                "Fixie.Tests.TestClasses.DisposalTests+DisposableTestClass.Pass passed.");

            DisposableTestClass.ConstructionCount.ShouldEqual(2);
            DisposableTestClass.DisposalCount.ShouldEqual(2);
        }

        public void ShouldFailCasesWhenDisposeThrowsExceptionsWithoutSuppressingAnyExceptions()
        {
            var listener = new StubListener();

            new SelfTestConvention().Execute(listener, typeof(DisposeThrowsTestClass));

            listener.ShouldHaveEntries(
                "Fixie.Tests.TestClasses.DisposalTests+DisposeThrowsTestClass.Fail failed: 'Fail' failed!" + Environment.NewLine +
                "    Secondary Failure: 'Dispose' failed!",
                "Fixie.Tests.TestClasses.DisposalTests+DisposeThrowsTestClass.Pass failed: 'Dispose' failed!");
        }

        class DisposableTestClass : IDisposable
        {
            public static int ConstructionCount { get; set; }
            public static int DisposalCount { get; set; }
            bool disposed;

            public DisposableTestClass()
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

        class DisposeThrowsTestClass : IDisposable
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