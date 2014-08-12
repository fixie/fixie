using System;
using System.Collections.Generic;

namespace Fixie.Execution.Behaviors
{
    public class CreateInstancePerCase : ClassBehavior
    {
        readonly Func<Type, object> testClassFactory;
        readonly BehaviorChain<Fixture> fixtureBehaviors;

        public CreateInstancePerCase(Func<Type, object> testClassFactory, BehaviorChain<Fixture> fixtureBehaviors)
        {
            this.testClassFactory = testClassFactory;
            this.fixtureBehaviors = fixtureBehaviors;
        }

        public void Execute(Class testClass, Action next)
        {
            foreach (var @case in testClass.Cases)
            {
                try
                {
                    PerformClassLifecycle(testClass, new[] { @case });
                }
                catch (Exception exception)
                {
                    @case.Fail(exception);
                }
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