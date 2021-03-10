namespace Fixie.Internal
{
    using System.Threading.Tasks;

    class DefaultExecution : Execution
    {
        public async Task RunAsync(TestAssembly testAssembly)
        {
            foreach (var test in testAssembly.Tests)
                await test.RunAsync();
        }
    }
}