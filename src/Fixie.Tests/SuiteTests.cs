using Should;
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

            var result = listener.State.ToResult();
            result.Total.ShouldEqual(2);
            result.Passed.ShouldEqual(2);
            result.Failed.ShouldEqual(0);

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