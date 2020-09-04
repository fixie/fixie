namespace Fixie.Internal
{
    class DefaultExecution : Execution
    {
        public void Execute(TestClass testClass)
        {
            foreach (var test in testClass.Tests)
                test.Run();
        }
    }
}