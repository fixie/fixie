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

            var fixture = new Fixture(testClass, instance, casesForThisInstance);
            fixtureBehaviors.Execute(fixture);

            var disposable = instance as IDisposable;
            if (disposable != null)
                disposable.Dispose();
        }
    }
}