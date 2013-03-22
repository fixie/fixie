using System;
using System.Linq;
using Should;
using Xunit;

namespace Fixie.Tests
{
    public class ClassFixtureTests
    {
        [Fact]
        public void ShouldBeNamedAfterTheGivenFixtureClass()
        {
            var fixtureClass = typeof(DiscoverySampleFixture);
            var fixture = new ClassFixture(fixtureClass);

            fixture.Name.ShouldEqual("Fixie.Tests.ClassFixtureTests+DiscoverySampleFixture");
        }

        [Fact]
        public void ShouldTreatPublicInstanceNoArgVoidMethodsAsCases()
        {
            var fixtureClass = typeof(DiscoverySampleFixture);
            var fixture = new ClassFixture(fixtureClass);

            fixture.Cases.Select(x => x.Name).ShouldEqual("Fixie.Tests.ClassFixtureTests+DiscoverySampleFixture.PublicInstanceNoArgsVoid");
        }

        [Fact]
        public void ShouldExecuteAllCases()
        {
            var listener = new StubListener();
            var fixtureClass = typeof(ExecutionSampleFixture);
            var fixture = new ClassFixture(fixtureClass);

            var result = fixture.Execute(listener);

            result.Total.ShouldEqual(5);
            result.Passed.ShouldEqual(3);
            result.Failed.ShouldEqual(2);

            listener.Entries.ShouldEqual("Fixie.Tests.ClassFixtureTests+ExecutionSampleFixture.FailingCaseA failed: Failing Case A",
                                         "Fixie.Tests.ClassFixtureTests+ExecutionSampleFixture.FailingCaseB failed: Failing Case B");
        }

        [Fact]
        public void ShouldFailAllCasesWhenFixtureConstructorCannotBeInvoked()
        {
            var listener = new StubListener();
            var fixtureClass = typeof(CannotInvokeConstructorSampleFixture);
            var fixture = new ClassFixture(fixtureClass);

            var result = fixture.Execute(listener);

            result.Total.ShouldEqual(2);
            result.Passed.ShouldEqual(0);
            result.Failed.ShouldEqual(2);

            listener.Entries.ShouldEqual(
                "Fixie.Tests.ClassFixtureTests+CannotInvokeConstructorSampleFixture.UnreachableCaseA failed: No parameterless constructor defined for this object.",
                "Fixie.Tests.ClassFixtureTests+CannotInvokeConstructorSampleFixture.UnreachableCaseB failed: No parameterless constructor defined for this object.");
        }

        [Fact]
        public void ShouldFailAllCasesWhenFixtureConstructorThrowsException()
        {
            var listener = new StubListener();
            var fixtureClass = typeof(ConstructorThrowsSampleFixture);
            var fixture = new ClassFixture(fixtureClass);

            var result = fixture.Execute(listener);

            result.Total.ShouldEqual(2);
            result.Passed.ShouldEqual(0);
            result.Failed.ShouldEqual(2);

            listener.Entries.ShouldEqual(
                "Fixie.Tests.ClassFixtureTests+ConstructorThrowsSampleFixture.UnreachableCaseA failed: Exception From Constructor",
                "Fixie.Tests.ClassFixtureTests+ConstructorThrowsSampleFixture.UnreachableCaseB failed: Exception From Constructor");
        }

        class DiscoverySampleFixture
        {
            public static int PublicStaticWithArgsWithReturn(int x) { return 0; }
            public static int PublicStaticNoArgsWithReturn() { return 0; }
            public static void PublicStaticWithArgsVoid(int x) { }
            public static void PublicStaticNoArgsVoid() { }

            public int PublicInstanceWithArgsWithReturn(int x) { return 0; }
            public int PublicInstanceNoArgsWithReturn() { return 0; }
            public void PublicInstanceWithArgsVoid(int x) { }
            public void PublicInstanceNoArgsVoid() { }

            private static int PrivateStaticWithArgsWithReturn(int x) { return 0; }
            private static int PrivateStaticNoArgsWithReturn() { return 0; }
            private static void PrivateStaticWithArgsVoid(int x) { }
            private static void PrivateStaticNoArgsVoid() { }

            private int PrivateInstanceWithArgsWithReturn(int x) { return 0; }
            private int PrivateInstanceNoArgsWithReturn() { return 0; }
            private void PrivateInstanceWithArgsVoid(int x) { }
            private void PrivateInstanceNoArgsVoid() { }
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