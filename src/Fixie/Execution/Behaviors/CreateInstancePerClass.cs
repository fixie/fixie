using System;
using System.Collections.Generic;
using System.Linq;

namespace Fixie.Execution.Behaviors
{
    public class CreateInstancePerClass : ClassBehavior
    {
        readonly Func<Type, object> testClassFactory;
        readonly BehaviorChain<InstanceExecution> instanceBehaviors;

        public CreateInstancePerClass(Func<Type, object> testClassFactory, BehaviorChain<InstanceExecution> instanceBehaviors)
        {
            this.testClassFactory = testClassFactory;
            this.instanceBehaviors = instanceBehaviors;
        }

        public void Execute(TestClass testClass, Action next)
        {
            try
            {
                PerformClassLifecycle(testClass.Type, testClass.Cases);
            }
            catch (Exception exception)
            {
                testClass.Fail(exception);
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