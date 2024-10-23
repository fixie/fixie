namespace Fixie;

public interface IExecution
{
    /// <summary>
    /// Runs the given set of discovered tests.
    /// </summary>
    Task Run(TestSuite testSuite);
}

public sealed class DefaultExecution : IExecution
{
    public async Task Run(TestSuite testSuite)
    {
        foreach (var test in testSuite.Tests)
            await test.Run();
    }
}

public sealed class ParallelExecution : IExecution
{
    public async Task Run(TestSuite testSuite)
    {
        await Parallel.ForEachAsync(testSuite.Tests, async (test, _) => await test.Run());
    }
}