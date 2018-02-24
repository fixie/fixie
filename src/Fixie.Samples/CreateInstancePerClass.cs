namespace Fixie.Samples
{
    using System;

    class CreateInstancePerClass : Lifecycle
    {
        public void Execute(TestClass testClass, Action<CaseAction> runCases)
        {
            var instance = Activator.CreateInstance(testClass.Type);

            runCases(@case => @case.Execute(instance));

            (instance as IDisposable)?.Dispose();
        }
    }
}
