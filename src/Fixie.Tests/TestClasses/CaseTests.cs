using Fixie.Conventions;

namespace Fixie.Tests.TestClasses
{
    public class CaseTests
    {
        public void ShouldPassUponSuccessfulExecution()
        {
            var listener = new StubListener();

            new SelfTestConvention().Execute(listener , typeof(PassTestClass));

            listener.ShouldHaveEntries(
                "Fixie.Tests.TestClasses.CaseTests+PassTestClass.Pass passed.");
        }

        public void ShouldFailWithOriginalExceptionWhenCaseMethodThrows()
        {
            var listener = new StubListener();

            new SelfTestConvention().Execute(listener, typeof(FailTestClass));

            listener.ShouldHaveEntries(
                "Fixie.Tests.TestClasses.CaseTests+FailTestClass.Fail failed: 'Fail' failed!");
        }

        public void ShouldPassOrFailCasesIndividually()
        {
            var listener = new StubListener();

            new SelfTestConvention().Execute(listener, typeof(PassFailTestClass));

            listener.ShouldHaveEntries(
                "Fixie.Tests.TestClasses.CaseTests+PassFailTestClass.FailA failed: 'FailA' failed!",
                "Fixie.Tests.TestClasses.CaseTests+PassFailTestClass.PassA passed.",
                "Fixie.Tests.TestClasses.CaseTests+PassFailTestClass.FailB failed: 'FailB' failed!",
                "Fixie.Tests.TestClasses.CaseTests+PassFailTestClass.PassB passed.",
                "Fixie.Tests.TestClasses.CaseTests+PassFailTestClass.PassC passed.");
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
    }
}