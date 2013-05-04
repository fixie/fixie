using System;
using Should;

namespace Fixie.Tests
{
    public class ClassFixtureTests
    {
        readonly DefaultConvention defaultConvention;

        public ClassFixtureTests()
        {
            defaultConvention = new DefaultConvention();
        }

        public void ShouldBeNamedAfterTheGivenFixtureClass()
        {
            var fixtureClass = typeof(ExecutionSampleFixture);
            var fixture = new ClassFixture(fixtureClass, defaultConvention);

            fixture.Name.ShouldEqual("Fixie.Tests.ClassFixtureTests+ExecutionSampleFixture");
        }

        public void ShouldExecuteAllCases()
        {
            var listener = new StubListener();
            var fixtureClass = typeof(ExecutionSampleFixture);
            var fixture = new ClassFixture(fixtureClass, defaultConvention);

            fixture.Execute(listener);

            listener.ShouldHaveEntries(
                "Fixie.Tests.ClassFixtureTests+ExecutionSampleFixture.FailingCaseA failed: Failing Case A",
                "Fixie.Tests.ClassFixtureTests+ExecutionSampleFixture.PassingCaseA passed.",
                "Fixie.Tests.ClassFixtureTests+ExecutionSampleFixture.FailingCaseB failed: Failing Case B",
                "Fixie.Tests.ClassFixtureTests+ExecutionSampleFixture.PassingCaseB passed.",
                "Fixie.Tests.ClassFixtureTests+ExecutionSampleFixture.PassingCaseC passed.");
        }

        public void ShouldFailAllCasesWhenFixtureConstructorCannotBeInvoked()
        {
            var listener = new StubListener();
            var fixtureClass = typeof(CannotInvokeConstructorSampleFixture);
            var fixture = new ClassFixture(fixtureClass, defaultConvention);

            fixture.Execute(listener);

            listener.ShouldHaveEntries(
                "Fixie.Tests.ClassFixtureTests+CannotInvokeConstructorSampleFixture.UnreachableCaseA failed: No parameterless constructor defined for this object.",
                "Fixie.Tests.ClassFixtureTests+CannotInvokeConstructorSampleFixture.UnreachableCaseB failed: No parameterless constructor defined for this object.");
        }

        public void ShouldFailAllCasesWithOriginalExceptionWhenFixtureConstructorThrowsException()
        {
            var listener = new StubListener();
            var fixtureClass = typeof(ConstructorThrowsSampleFixture);
            var fixture = new ClassFixture(fixtureClass, defaultConvention);

            fixture.Execute(listener);

            listener.ShouldHaveEntries(
                "Fixie.Tests.ClassFixtureTests+ConstructorThrowsSampleFixture.UnreachableCaseA failed: Exception From Constructor",
                "Fixie.Tests.ClassFixtureTests+ConstructorThrowsSampleFixture.UnreachableCaseB failed: Exception From Constructor");
        }

        public void ShouldDisposeFixtureInstancesWhenDisposable()
        {
            var listener = new StubListener();
            var fixtureClass = typeof(DisposableSampleFixture);
            var fixture = new ClassFixture(fixtureClass, defaultConvention);

            DisposableSampleFixture.ConstructionCount = 0;
            DisposableSampleFixture.DisposalCount = 0;

            fixture.Execute(listener);

            listener.ShouldHaveEntries(
                "Fixie.Tests.ClassFixtureTests+DisposableSampleFixture.FailingCase failed: Failing Case",
                "Fixie.Tests.ClassFixtureTests+DisposableSampleFixture.PassingCase passed.");

            DisposableSampleFixture.ConstructionCount.ShouldEqual(2);
            DisposableSampleFixture.DisposalCount.ShouldEqual(2);
        }

        public void ShouldFailCasesWhenDisposeThrowsExceptionsWithoutSuppressingAnyExceptions()
        {
            var listener = new StubListener();
            var fixtureClass = typeof(DisposeThrowsSampleFixture);
            var fixture = new ClassFixture(fixtureClass, defaultConvention);

            fixture.Execute(listener);

            listener.ShouldHaveEntries(
                "Fixie.Tests.ClassFixtureTests+DisposeThrowsSampleFixture.FailingCase failed: Failing Case" + Environment.NewLine +
                "    Secondary Failure: Exception From IDisposable.Dispose().",
                "Fixie.Tests.ClassFixtureTests+DisposeThrowsSampleFixture.PassingCase failed: Exception From IDisposable.Dispose().");
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

            public void PassingCase()
            {
            }
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

            public void PassingCase()
            {
            }
        }
    }
}