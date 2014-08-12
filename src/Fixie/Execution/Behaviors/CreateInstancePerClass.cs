using System;
using System.Collections.Generic;

namespace Fixie.Execution.Behaviors
{
    public class CreateInstancePerClass : ClassBehavior
    {
        readonly Func<Type, object> testClassFactory;
        readonly BehaviorChain<Fixture> fixtureBehaviors;

        public CreateInstancePerClass(Func<Type, object> testClassFactory, BehaviorChain<Fixture> fixtureBehaviors)
        {
            this.testClassFactory = testClassFactory;
            this.fixtureBehaviors = fixtureBehaviors;
        }

        public void Execute(Class testClass, Action next)
        {
            try
            {
                PerformClassLifecycle(testClass, testClass.Cases);
            }
            catch (Exception exception)
            {
                testClass.Fail(exception);
            }
        }

        void PerformClassLifecycle(Class testClass, IReadOnlyList<Case> casesForThisInstance)
        {
            var instance = testClassFactory(testClass.Type);

            var fixture = new Fixture(testClass, instance, casesForThisInstance);
            fixtureBehaviors.Execute(fixture);

            var disposable = instance as IDisposable;
            if (disposable != null)
                disposable.Dispose();
        }
    }
}