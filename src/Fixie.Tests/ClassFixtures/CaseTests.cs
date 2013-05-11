using System;

namespace Fixie.Tests.ClassFixtures
{
    public class CaseTests
    {
        public void ShouldPassIffCaseThrowsNoExceptions()
        {
            var listener = new StubListener();
            var defaultConvention = new DefaultConvention();
            var fixtureClass = typeof(ExecutionSampleFixture);
            var fixture = new ClassFixture(fixtureClass, defaultConvention);

            fixture.Execute(listener);

            listener.ShouldHaveEntries(
                "Fixie.Tests.ClassFixtures.CaseTests+ExecutionSampleFixture.FailingCaseA failed: Failing Case A",
                "Fixie.Tests.ClassFixtures.CaseTests+ExecutionSampleFixture.PassingCaseA passed.",
                "Fixie.Tests.ClassFixtures.CaseTests+ExecutionSampleFixture.FailingCaseB failed: Failing Case B",
                "Fixie.Tests.ClassFixtures.CaseTests+ExecutionSampleFixture.PassingCaseB passed.",
                "Fixie.Tests.ClassFixtures.CaseTests+ExecutionSampleFixture.PassingCaseC passed.");
        }

        class ExecutionSampleFixture
        {
            public void FailingCaseA() { throw new Exception("Failing Case A"); }

            public void PassingCaseA() { }

            public void FailingCaseB() { throw new Exception("Failing Case B"); }

            public void PassingCaseB() { }

            public void PassingCaseC() { }
        }
    }
}