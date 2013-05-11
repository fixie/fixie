using System;

namespace Fixie.Tests.ClassFixtures
{
    public class CaseTests
    {
        public void ShouldPassUponSuccessfulExecution()
        {
            var fixture = new ClassFixture(typeof(PassFixture), new DefaultConvention());
            var listener = new StubListener();

            fixture.Execute(listener);

            listener.ShouldHaveEntries(
                "Fixie.Tests.ClassFixtures.CaseTests+PassFixture.Pass passed.");
        }

        public void ShouldFailWithOriginalExceptionWhenCaseMethodThrows()
        {
            var fixture = new ClassFixture(typeof(FailFixture), new DefaultConvention());
            var listener = new StubListener();

            fixture.Execute(listener);

            listener.ShouldHaveEntries(
                "Fixie.Tests.ClassFixtures.CaseTests+FailFixture.Fail failed: Exception of type " +
                "'Fixie.Tests.ClassFixtures.CaseTests+MethodInvokedException' was thrown.");
        }

        public void ShouldPassOrFailCasesIndividually()
        {
            var fixture = new ClassFixture(typeof(PassFailFixture), new DefaultConvention());
            var listener = new StubListener();

            fixture.Execute(listener);

            listener.ShouldHaveEntries(
                "Fixie.Tests.ClassFixtures.CaseTests+PassFailFixture.FailingCaseA failed: Failing Case A",
                "Fixie.Tests.ClassFixtures.CaseTests+PassFailFixture.PassingCaseA passed.",
                "Fixie.Tests.ClassFixtures.CaseTests+PassFailFixture.FailingCaseB failed: Failing Case B",
                "Fixie.Tests.ClassFixtures.CaseTests+PassFailFixture.PassingCaseB passed.",
                "Fixie.Tests.ClassFixtures.CaseTests+PassFailFixture.PassingCaseC passed.");
        }

        class PassFixture
        {
            public void Pass() { }
        }

        class FailFixture
        {
            public void Fail()
            {
                throw new MethodInvokedException();
            }
        }

        class PassFailFixture
        {
            public void FailingCaseA() { throw new Exception("Failing Case A"); }

            public void PassingCaseA() { }

            public void FailingCaseB() { throw new Exception("Failing Case B"); }

            public void PassingCaseB() { }

            public void PassingCaseC() { }
        }

        class MethodInvokedException : Exception { }
    }
}