using Fixie.Conventions;

namespace Fixie.Tests.TestClasses
{
    public class NullaryMethodTests
    {
        public void ShouldPassUponSuccessfulExecution()
        {
            var listener = new StubListener();

            new SelfTestConvention().Execute(listener , typeof(PassTestClass));

            listener.Entries.ShouldEqual(
                "Fixie.Tests.TestClasses.NullaryMethodTests+PassTestClass.Pass passed.");
        }

        public void ShouldFailWithOriginalExceptionWhenCaseMethodThrows()
        {
            var listener = new StubListener();

            new SelfTestConvention().Execute(listener, typeof(FailTestClass));

            listener.Entries.ShouldEqual(
                "Fixie.Tests.TestClasses.NullaryMethodTests+FailTestClass.Fail failed: 'Fail' failed!");
        }

        public void ShouldPassOrFailCasesIndividually()
        {
            var listener = new StubListener();

            new SelfTestConvention().Execute(listener, typeof(PassFailTestClass));

            listener.Entries.ShouldEqual(
                "Fixie.Tests.TestClasses.NullaryMethodTests+PassFailTestClass.FailA failed: 'FailA' failed!",
                "Fixie.Tests.TestClasses.NullaryMethodTests+PassFailTestClass.FailB failed: 'FailB' failed!",
                "Fixie.Tests.TestClasses.NullaryMethodTests+PassFailTestClass.PassA passed.",
                "Fixie.Tests.TestClasses.NullaryMethodTests+PassFailTestClass.PassB passed.",
                "Fixie.Tests.TestClasses.NullaryMethodTests+PassFailTestClass.PassC passed.");
        }

        public void ShouldFailWhenTestClassConstructorCannotBeInvoked()
        {
            var listener = new StubListener();

            new SelfTestConvention().Execute(listener, typeof(CannotInvokeConstructorTestClass));

            listener.Entries.ShouldEqual(
                "Fixie.Tests.TestClasses.NullaryMethodTests+CannotInvokeConstructorTestClass.UnreachableCase failed: No parameterless constructor defined for this object.");
        }

        class PassTestClass
        {
            public void Pass() { }
        }

        class FailTestClass
        {
            public void Fail()
            {
                throw new FailureException();
            }
        }

        class PassFailTestClass
        {
            public void FailA() { throw new FailureException(); }

            public void PassA() { }

            public void FailB() { throw new FailureException(); }

            public void PassB() { }

            public void PassC() { }
        }

        class CannotInvokeConstructorTestClass
        {
            public CannotInvokeConstructorTestClass(int argument)
            {
            }

            public void UnreachableCase() { }
        }
    }
}