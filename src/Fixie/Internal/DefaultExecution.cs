namespace Fixie.Internal
{
    class DefaultExecution : Execution
    {
        public void Execute(TestClass testClass)
            => testClass.RunTests(test => test.Run());
    }
}