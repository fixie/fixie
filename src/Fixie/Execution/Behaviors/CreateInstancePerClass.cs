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

        public void Execute(ClassExecution classExecution, Action next)
        {
            try
            {
                PerformClassLifecycle(classExecution.TestClass, classExecution.Cases);
            }
            catch (Exception exception)
            {
                classExecution.Fail(exception);
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