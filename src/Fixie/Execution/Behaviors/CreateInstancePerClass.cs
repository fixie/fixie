using System;
using System.Collections.Generic;
using System.Linq;

namespace Fixie.Execution.Behaviors
{
    public class CreateInstancePerClass : ClassBehavior
    {
        readonly Func<Type, object> testClassFactory;
        readonly BehaviorChain<Fixture> instanceBehaviors;

        public CreateInstancePerClass(Func<Type, object> testClassFactory, BehaviorChain<Fixture> instanceBehaviors)
        {
            this.testClassFactory = testClassFactory;
            this.instanceBehaviors = instanceBehaviors;
        }

        public void Execute(Class @class, Action next)
        {
            try
            {
                PerformClassLifecycle(@class.Type, @class.Cases);
            }
            catch (Exception exception)
            {
                @class.Fail(exception);
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