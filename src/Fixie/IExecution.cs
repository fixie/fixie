namespace Fixie;

public interface IExecution
{
    /// <summary>
    /// Runs the given set of discovered tests.
    /// </summary>
    Task Run(TestSuite testSuite);
}

/// <summary>
/// Run tests sequentially, instantiating the test class once per test method.
/// </summary>
public sealed class DefaultExecution : IExecution
{
    public async Task Run(TestSuite testSuite)
    {
        foreach (var test in testSuite.Tests)
            await test.Run();
    }
}

/// <summary>
/// Run tests in parallel, instantiating the test class once per test method.
/// 
/// <para>
/// Use with extreme caution, as unintentionally dependent tests may
/// interfere with each other.
/// </para>
/// </summary>
public sealed class ParallelExecution : IExecution
{
    public async Task Run(TestSuite testSuite)
    {
        await Parallel.ForEachAsync(testSuite.Tests, async (test, _) => await test.Run());
    }
}