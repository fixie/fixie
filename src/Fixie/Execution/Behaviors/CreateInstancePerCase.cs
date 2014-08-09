using System;
using System.Collections.Generic;

namespace Fixie.Execution.Behaviors
{
    public class CreateInstancePerCase : ClassBehavior
    {
        readonly Func<Type, object> testClassFactory;
        readonly BehaviorChain<Fixture> instanceBehaviors;

        public CreateInstancePerCase(Func<Type, object> testClassFactory, BehaviorChain<Fixture> instanceBehaviors)
        {
            this.testClassFactory = testClassFactory;
            this.instanceBehaviors = instanceBehaviors;
        }

        public void Execute(Class @class, Action next)
        {
            foreach (var @case in @class.Cases)
            {
                try
                {
                    PerformClassLifecycle(@class.Type, new[] { @case });
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

            var instanceExecution = new Fixture(testClass, instance, casesForThisInstance);
            instanceBehaviors.Execute(instanceExecution);

            var disposable = instance as IDisposable;
            if (disposable != null)
                disposable.Dispose();
        }
    }
}