namespace Fixie.Internal
{
    class DefaultExecution : Execution
    {
        public void Execute(TestClass testClass)
        {
            testClass.RunTests(@case =>
            {
                var instance = testClass.Construct();

                @case.Execute(instance);

                instance.Dispose();
            });
        }
    }
}