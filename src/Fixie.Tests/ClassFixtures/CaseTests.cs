using Fixie.Conventions;

namespace Fixie.Tests.ClassFixtures
{
    public class CaseTests
    {
        public void ShouldPassUponSuccessfulExecution()
        {
            var listener = new StubListener();

            new SelfTestConvention().Execute(listener , typeof(PassFixture));

            listener.ShouldHaveEntries(
                "Fixie.Tests.ClassFixtures.CaseTests+PassFixture.Pass passed.");
        }

        public void ShouldFailWithOriginalExceptionWhenCaseMethodThrows()
        {
            var listener = new StubListener();

            new SelfTestConvention().Execute(listener, typeof(FailFixture));

            listener.ShouldHaveEntries(
                "Fixie.Tests.ClassFixtures.CaseTests+FailFixture.Fail failed: 'Fail' failed!");
        }

        public void ShouldPassOrFailCasesIndividually()
        {
            var listener = new StubListener();

            new SelfTestConvention().Execute(listener, typeof(PassFailFixture));

            listener.ShouldHaveEntries(
                "Fixie.Tests.ClassFixtures.CaseTests+PassFailFixture.FailA failed: 'FailA' failed!",
                "Fixie.Tests.ClassFixtures.CaseTests+PassFailFixture.PassA passed.",
                "Fixie.Tests.ClassFixtures.CaseTests+PassFailFixture.FailB failed: 'FailB' failed!",
                "Fixie.Tests.ClassFixtures.CaseTests+PassFailFixture.PassB passed.",
                "Fixie.Tests.ClassFixtures.CaseTests+PassFailFixture.PassC passed.");
        }

        class PassFixture
        {
            public void Pass() { }
        }

        class FailFixture
        {
            public void Fail()
            {
                throw new FailureException();
            }
        }

        class PassFailFixture
        {
            public void FailA() { throw new FailureException(); }

            public void PassA() { }

            public void FailB() { throw new FailureException(); }

            public void PassB() { }

            public void PassC() { }
        }
    }
}