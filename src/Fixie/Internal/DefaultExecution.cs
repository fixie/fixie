namespace Fixie.Internal
{
    using System.Threading.Tasks;

    class DefaultExecution : IExecution
    {
        public async Task RunAsync(TestSuite testSuite)
        {
            foreach (var test in testSuite.Tests)
                await test.RunAsync();
        }
    }
}