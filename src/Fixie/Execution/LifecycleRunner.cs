namespace Fixie.Execution
{
    using System;
    using System.Collections.Generic;
    using Behaviors;

    class LifecycleRunner
    {
        readonly Lifecycle lifecycle;

        public LifecycleRunner(Lifecycle lifecycle)
            => this.lifecycle = lifecycle;

        public void Execute(Type testClass, IReadOnlyList<Case> cases)
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
                foreach (var @case in cases)
                    @case.Fail(exception);
            }
        }
    }
}