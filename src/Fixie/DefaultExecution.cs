namespace Fixie;

public sealed class DefaultExecution : IExecution
{
    public async Task Run(TestSuite testSuite)
    {
        foreach (var test in testSuite.Tests)
            await test.Run();
    }
}