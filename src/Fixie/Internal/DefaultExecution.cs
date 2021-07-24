namespace Fixie.Internal
{
    using System.Threading.Tasks;

    class DefaultExecution : IExecution
    {
        public async Task RunAsync(TestAssembly testAssembly)
        {
            foreach (var test in testAssembly.Tests)
                await test.RunAsync();
        }
    }
}