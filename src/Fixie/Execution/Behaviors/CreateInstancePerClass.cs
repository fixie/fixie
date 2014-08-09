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

        public void Execute(Class @class, Action next)
        {
            try
            {
                PerformClassLifecycle(@class, @class.Cases);
            }
            catch (Exception exception)
            {
                @class.Fail(exception);
            }
        }

        void PerformClassLifecycle(Class @class, IReadOnlyList<Case> casesForThisInstance)
        {
            var instance = testClassFactory(@class.Type);

            var fixture = new Fixture(@class, instance, casesForThisInstance);
            fixtureBehaviors.Execute(fixture);

            var disposable = instance as IDisposable;
            if (disposable != null)
                disposable.Dispose();
        }
    }
}