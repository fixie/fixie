namespace Fixie.Samples
{
    using System;

    class CreateInstancePerClass : Lifecycle
    {
        public void Execute(TestClass testClass, Action<CaseAction> runCases)
        {
            var instance = testClass.Construct();

            runCases(@case => @case.Execute(instance));

            (instance as IDisposable)?.Dispose();
        }
    }
}
