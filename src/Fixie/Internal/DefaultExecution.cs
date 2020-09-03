namespace Fixie.Internal
{
    using System;

    class DefaultExecution : Execution
    {
        public void Execute(TestClass testClass)
        {
            foreach (var test in testClass.Tests)
            {
                try
                {
                    test.Run();
                }
                catch (Exception exception)
                {
                    test.Fail(exception);
                }
            }
        }
    }
}