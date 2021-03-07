namespace Fixie.Internal
{
    using System.Threading.Tasks;

    class DefaultExecution : Execution
    {
        public async Task RunAsync(TestAssembly testAssembly)
        {
            foreach (var testClass in testAssembly.TestClasses)
                foreach (var test in testClass.Tests)
                    await test.RunAsync();
        }
    }
}