namespace Fixie.Tests.Cases
{
    using Assertions;
    using static Utility;

    public class BasicCaseTests
    {
        public void ShouldPassUponSuccessfulExecution()
        {
            Run<PassTestClass>()
                .ShouldBe(
                    For<PassTestClass>(".Pass passed"));
        }

        public void ShouldFailWithOriginalExceptionWhenCaseMethodThrows()
        {
            Run<FailTestClass>()
                .ShouldBe(
                    For<FailTestClass>(".Fail failed: 'Fail' failed!"));
        }

        public void ShouldPassOrFailCasesIndividually()
        {
            Run<PassFailTestClass>()
                .ShouldBe(
                    For<PassFailTestClass>(
                        ".FailA failed: 'FailA' failed!",
                        ".FailB failed: 'FailB' failed!",
                        ".PassA passed",
                        ".PassB passed",
                        ".PassC passed"));
        }

        public void ShouldFailWhenTestClassConstructorCannotBeInvoked()
        {
            Run<CannotInvokeConstructorTestClass>()
                .ShouldBe(
                    For<CannotInvokeConstructorTestClass>(
                        ".UnreachableCase skipped",
                        ".UnreachableCase failed: No parameterless constructor defined " +
                        $"for type '{FullName<CannotInvokeConstructorTestClass>()}'."));
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

            public void FailB() { throw new FailureException(); }

            public void PassA() { }

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