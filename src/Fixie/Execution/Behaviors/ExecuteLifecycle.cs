namespace Fixie.Execution.Behaviors
{
    using System;
    using System.Collections.Generic;

    class ExecuteLifecycle
    {
        readonly Lifecycle lifecycle;

        public ExecuteLifecycle(Lifecycle lifecycle)
        {
            this.lifecycle = lifecycle;
        }

        public void Execute(Type testClass, IReadOnlyList<Case> cases)
        {
            var timeClassExecution = new TimeClassExecution();

            timeClassExecution.Execute(cases, () =>
            {
                try
                {
                    lifecycle.Execute(testClass, caseLifecycle =>
                    {
                        new ExecuteCases().Execute(caseLifecycle, cases);
                    });
                }
                catch (Exception exception)
                {
                    foreach (var @case in cases)
                        @case.Fail(exception);
                }
            });
        }
    }
}