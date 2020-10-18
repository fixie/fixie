namespace Fixie.Internal
{
    using System.Threading.Tasks;

    class DefaultExecution : Execution
    {
        public async Task ExecuteAsync(TestClass testClass)
        {
            foreach (var test in testClass.Tests)
                await test.RunAsync();
        }
    }
}