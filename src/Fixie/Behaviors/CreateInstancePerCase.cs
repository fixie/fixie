using System;
using System.Collections.Generic;

namespace Fixie.Behaviors
{
    public class CreateInstancePerCase : ClassBehavior
    {
        readonly Func<Type, object> testClassFactory;
        readonly BehaviorChain<InstanceExecution> instanceBehaviorChain;

        public CreateInstancePerCase(Func<Type, object> testClassFactory, BehaviorChain<InstanceExecution> instanceBehaviorChain)
        {
            this.testClassFactory = testClassFactory;
            this.instanceBehaviorChain = instanceBehaviorChain;
        }

        public void Execute(ClassExecution classExecution, Action next)
        {
            foreach (var caseExecution in classExecution.CaseExecutions)
            {
                try
                {
                    PerformClassLifecycle(classExecution.TestClass, new[] { caseExecution });
                }
                catch (Exception exception)
                {
                    caseExecution.Fail(exception);
                }
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