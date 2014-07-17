using System;
using System.Collections.Generic;

namespace Fixie.Behaviors
{
    public class CreateInstancePerClass : ClassBehavior
    {
        readonly Func<Type, object> testClassFactory;
        readonly BehaviorChain<InstanceExecution> instanceBehaviorChain;

        public CreateInstancePerClass(Func<Type, object> testClassFactory, BehaviorChain<InstanceExecution> instanceBehaviorChain)
        {
            this.testClassFactory = testClassFactory;
            this.instanceBehaviorChain = instanceBehaviorChain;
        }

        public void Execute(ClassExecution classExecution, Action next)
        {
            try
            {
                PerformClassLifecycle(classExecution.TestClass, classExecution.CaseExecutions);
            }
            catch (Exception exception)
            {
                classExecution.Fail(exception);
            }
        }

        void PerformClassLifecycle(Type testClass, IReadOnlyList<CaseExecution> caseExecutionsForThisInstance)
        {
            var instance = testClassFactory(testClass);

            var instanceExecution = new InstanceExecution(testClass, instance, caseExecutionsForThisInstance);
            instanceBehaviorChain.Execute(instanceExecution);

            var disposable = instance as IDisposable;
            if (disposable != null)
                disposable.Dispose();
        }
    }
}