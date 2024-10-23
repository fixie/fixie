namespace Fixie;

public sealed class DefaultExecution : IExecution
{
    /// <summary>
    /// When enabled, tests may run in parallel. Use with extreme caution,
    /// as unintentionally dependent tests may interfere with each other.
    /// </summary>
    public bool Parallel { get; init; }

    public async Task Run(TestSuite testSuite)
    {
        if (Parallel)
        {
            await System.Threading.Tasks.Parallel.ForEachAsync(testSuite.Tests, async (test, _) => await test.Run());
        }
        else
        {
            foreach (var test in testSuite.Tests)
                await test.Run();
        }
    }
}