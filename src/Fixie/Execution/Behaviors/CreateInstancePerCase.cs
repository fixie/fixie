using System;
using System.Collections.Generic;

namespace Fixie.Execution.Behaviors
{
    public class CreateInstancePerCase : ClassBehavior
    {
        readonly Func<Type, object> testClassFactory;
        readonly BehaviorChain<InstanceExecution> instanceBehaviors;

        public CreateInstancePerCase(Func<Type, object> testClassFactory, BehaviorChain<InstanceExecution> instanceBehaviors)
        {
            this.testClassFactory = testClassFactory;
            this.instanceBehaviors = instanceBehaviors;
        }

        public void Execute(ClassExecution classExecution, Action next)
        {
            foreach (var @case in classExecution.Cases)
            {
                try
                {
                    PerformClassLifecycle(classExecution.Type, new[] { @case });
                }
                catch (Exception exception)
                {
                    @case.Fail(exception);
                }
            }
        }

        void PerformClassLifecycle(Type testClass, IReadOnlyList<Case> casesForThisInstance)
        {
            var instance = testClassFactory(testClass);

            var instanceExecution = new InstanceExecution(testClass, instance, casesForThisInstance);
            instanceBehaviors.Execute(instanceExecution);

            var disposable = instance as IDisposable;
            if (disposable != null)
                disposable.Dispose();
        }
    }
}