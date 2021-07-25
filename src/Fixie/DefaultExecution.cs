namespace Fixie
{
    using System.Threading.Tasks;

    public sealed class DefaultExecution : IExecution
    {
        public async Task Run(TestSuite testSuite)
        {
            foreach (var test in testSuite.Tests)
                await test.RunAsync();
        }
    }
}