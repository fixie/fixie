namespace Fixie.Tests;

public class ResultEmittingTests : InstrumentedExecutionTests
{
    class SampleTestClass
    {
        public void Test0() { throw new ShouldBeUnreachableException(); }
        public void Test1() { throw new ShouldBeUnreachableException(); }
    }

    class ResultEmittingExecution : IExecution
    {
        public async Task Run(TestSuite testSuite)
        {
            var exception = new Exception("Non-invocation Failure");

            foreach (var test in testSuite.Tests.Where(x => x.Name.EndsWith("Test0")))
            {
                await test.Pass();
                await test.Fail(exception);
                await test.Skip("Explicit skip reason.");

                await test.Pass(new object[] {0, 'A'});
                await test.Fail(new object[] {1, 'B'}, exception);
                await test.Skip(new object[] {2, 'C'}, reason: "");
            }
        }
    }

    public async Task ShouldSupportExplicitlyEmittingResultsWithoutNecessarilyInvokingTestMethods()
    {
        var output = await Run<SampleTestClass, ResultEmittingExecution>();

        output.ShouldHaveResults(
            "SampleTestClass.Test0 passed",
            "SampleTestClass.Test0 failed: Non-invocation Failure",
            "SampleTestClass.Test0 skipped: Explicit skip reason.",
            
            "SampleTestClass.Test0(0, 'A') passed",
            "SampleTestClass.Test0(1, 'B') failed: Non-invocation Failure",
            "SampleTestClass.Test0(2, 'C') skipped: This test was explicitly skipped, but no reason was provided.",
            
            "SampleTestClass.Test1 skipped: This test did not run.");
    }
}