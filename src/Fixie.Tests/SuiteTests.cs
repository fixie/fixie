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
            var convention = new Suite(typeof(ExecutionSampleTests));

            var result = convention.Execute(listener);

            result.Total.ShouldEqual(2);
            result.Passed.ShouldEqual(2);
            result.Failed.ShouldEqual(0);

            listener.Entries.ShouldBeEmpty();
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