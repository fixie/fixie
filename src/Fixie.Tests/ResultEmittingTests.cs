namespace Fixie.Tests
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    public class ResultEmittingTests : InstrumentedExecutionTests
    {
        class SampleTestClass
        {
            public void Test0() { throw new ShouldBeUnreachableException(); }
            public void Test1() { throw new ShouldBeUnreachableException(); }
        }

        class ResultEmittingExecution : Execution
        {
            public async Task RunAsync(TestAssembly testAssembly)
            {
                var exception = new Exception("Non-invocation Failure");

                foreach (var test in testAssembly.Tests.Where(x => x.Name.EndsWith("Test0")))
                {
                    await test.PassAsync();
                    await test.FailAsync(exception);
                    await test.SkipAsync("Explicit skip reason.");

                    await test.PassAsync(new object[] {0, 'A'});
                    await test.FailAsync(new object[] {1, 'B'}, exception);
                    await test.SkipAsync(new object[] {2, 'C'}, "Explicit skip reason.");
                }
            }
        }

        public async Task ShouldSupportExplicitlyEmittingResultsWithoutNecessarilyInvokingTestMethods()
        {
            var output = await RunAsync<SampleTestClass, ResultEmittingExecution>();

            output.ShouldHaveResults(
                "SampleTestClass.Test0 passed",
                "SampleTestClass.Test0 failed: Non-invocation Failure",
                "SampleTestClass.Test0 skipped: Explicit skip reason.",
                
                "SampleTestClass.Test0(0, 'A') passed",
                "SampleTestClass.Test0(1, 'B') failed: Non-invocation Failure",
                "SampleTestClass.Test0(2, 'C') skipped: Explicit skip reason.",
                
                "SampleTestClass.Test1 skipped: This test did not run.");
        }
    }
}