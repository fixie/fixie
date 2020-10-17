namespace Fixie.Internal
{
    using System.Threading.Tasks;

    class DefaultExecution : Execution
    {
        public async Task Execute(TestClass testClass)
        {
            foreach (var test in testClass.Tests)
                await test.Run();
        }
    }
}