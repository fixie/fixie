using Xunit;

namespace Fixie.Tests
{
    public class SuiteTests
    {
        [Fact]
        public void ShouldExecuteAllCasesInAllFixtures()
        {
            var listener = new StubListener();
            var convention = new DefaultConvention();
            var suite = new Suite(convention, typeof(ExecutionSampleTests));

            suite.Execute(listener);

            listener.Entries.ShouldEqual("Fixie.Tests.SuiteTests+ExecutionSampleTests.PassA passed.",
                                         "Fixie.Tests.SuiteTests+ExecutionSampleTests.PassB passed.");
        }

        public class ExecutionSampleTests
        {
            [Fact]
            public void PassA() { }

            [Fact]
            public void PassB() { }
        }
    }
}