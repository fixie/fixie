namespace Fixie.Samples
{
    using System;

    class CreateInstancePerClass : Lifecycle
    {
        public void Execute(RunContext runContext, Action<CaseAction> runCases)
        {
            var instance = Activator.CreateInstance(runContext.TestClass);

            runCases(@case => @case.Execute(instance));

            (instance as IDisposable)?.Dispose();
        }
    }
}
