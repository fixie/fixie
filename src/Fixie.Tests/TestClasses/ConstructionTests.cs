using Fixie.Conventions;

namespace Fixie.Tests.TestClasses
{
    public class ConstructionTests
    {
        public void ShouldFailAllCasesWhenTestClassConstructorCannotBeInvoked()
        {
            //This ensures we handle a case that LifecycleTests CANNOT test.

            var listener = new StubListener();

            new SelfTestConvention().Execute(listener, typeof(CannotInvokeConstructorTestClass));

            listener.ShouldHaveEntries(
                "Fixie.Tests.TestClasses.ConstructionTests+CannotInvokeConstructorTestClass.UnreachableCaseA failed: No parameterless constructor defined for this object.",
                "Fixie.Tests.TestClasses.ConstructionTests+CannotInvokeConstructorTestClass.UnreachableCaseB failed: No parameterless constructor defined for this object.");
        }

        class CannotInvokeConstructorTestClass
        {
            public CannotInvokeConstructorTestClass(int argument)
            {
            }

            public void UnreachableCaseA() { }
            public void UnreachableCaseB() { }
        }
    }
}