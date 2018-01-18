namespace Fixie.Samples
{
    using System;

    class CreateInstancePerClass : Lifecycle
    {
        public void Execute(Type testClass, Action<CaseAction> runCases)
        {
            var instance = Activator.CreateInstance(testClass);

            runCases(@case => @case.Execute(instance));

            (instance as IDisposable)?.Dispose();
        }
    }
}
