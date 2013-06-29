using Fixie.Conventions;
using Should;

namespace Fixie.Tests.TestClasses
{
    public class ConstructionTests
    {
        public void ShouldConstructInstancePerCase()
        {
            var listener = new StubListener();

            ConstructibleTestClass.ConstructionCount = 0;

             new SelfTestConvention().Execute(listener, typeof(ConstructibleTestClass));

            listener.ShouldHaveEntries(
                "Fixie.Tests.TestClasses.ConstructionTests+ConstructibleTestClass.Fail failed: 'Fail' failed!",
                "Fixie.Tests.TestClasses.ConstructionTests+ConstructibleTestClass.Pass passed.");

            ConstructibleTestClass.ConstructionCount.ShouldEqual(2);
        }

        public void ShouldFailAllCasesWhenTestClassConstructorCannotBeInvoked()
        {
            var listener = new StubListener();

            new SelfTestConvention().Execute(listener, typeof(CannotInvokeConstructorTestClass));

            listener.ShouldHaveEntries(
                "Fixie.Tests.TestClasses.ConstructionTests+CannotInvokeConstructorTestClass.UnreachableCaseA failed: No parameterless constructor defined for this object.",
                "Fixie.Tests.TestClasses.ConstructionTests+CannotInvokeConstructorTestClass.UnreachableCaseB failed: No parameterless constructor defined for this object.");
        }

        public void ShouldFailAllCasesWithOriginalExceptionWhenTestClassConstructorThrowsException()
        {
            var listener = new StubListener();

            new SelfTestConvention().Execute(listener, typeof(ConstructorThrowsTestClass));

            listener.ShouldHaveEntries(
                "Fixie.Tests.TestClasses.ConstructionTests+ConstructorThrowsTestClass.UnreachableCaseA failed: '.ctor' failed!",
                "Fixie.Tests.TestClasses.ConstructionTests+ConstructorThrowsTestClass.UnreachableCaseB failed: '.ctor' failed!");
        }

        class ConstructibleTestClass
        {
            public static int ConstructionCount { get; set; }

            public ConstructibleTestClass()
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

        class CannotInvokeConstructorTestClass
        {
            public CannotInvokeConstructorTestClass(int argument)
            {
            }

            public void UnreachableCaseA() { }
            public void UnreachableCaseB() { }
        }

        class ConstructorThrowsTestClass
        {
            public ConstructorThrowsTestClass()
            {
                throw new FailureException();
            }

            public void UnreachableCaseA() { }
            public void UnreachableCaseB() { }
        }
    }
}