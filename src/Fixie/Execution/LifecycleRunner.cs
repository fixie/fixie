namespace Fixie.Execution
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Behaviors;

    class LifecycleRunner
    {
        readonly Lifecycle lifecycle;

        public LifecycleRunner(Lifecycle lifecycle)
            => this.lifecycle = lifecycle;

        public void Execute(Type testClass, IReadOnlyList<Case> cases)
        {
            TimeClassExecution.Execute(cases, () =>
            {
                try
                {
                    lifecycle.Execute(testClass, caseLifecycle =>
                    {
                        ExecuteCases.Execute(cases, caseLifecycle);
                    });
                }
                catch (Exception exception)
                {
                    foreach (var case1 in cases)
                        case1.Fail(exception);
                }
            });
        }
    }
}