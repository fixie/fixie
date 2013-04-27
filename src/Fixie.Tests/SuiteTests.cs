namespace Fixie.Tests
{
    public class SuiteTests
    {
        public void ShouldExecuteAllCasesInAllFixtures()
        {
            var listener = new StubListener();
            var convention = new DefaultConvention();
            var suite = new Suite(convention, typeof(ExecutionSampleTests));

            suite.Execute(listener);

            listener.ShouldHaveEntries("Fixie.Tests.SuiteTests+ExecutionSampleTests.PassA passed.",
                                       "Fixie.Tests.SuiteTests+ExecutionSampleTests.PassB passed.");
        }

        public class ExecutionSampleTests
        {
            public void PassA() { }
            public void PassB() { }
        }
    }
}