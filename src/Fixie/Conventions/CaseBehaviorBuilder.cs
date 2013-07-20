using System;
using System.Reflection;
using Fixie.Behaviors;

namespace Fixie.Conventions
{
    public delegate void CaseBehaviorAction(Case @case, object instance, CaseBehavior inner);
    public delegate ExceptionList MethodAction(MethodInfo method, object instance);

    public class CaseBehaviorBuilder
    {
        public CaseBehaviorBuilder()
        {
            Behavior = new Invoke();
        }

        public CaseBehavior Behavior { get; private set; }

        public CaseBehaviorBuilder Wrap(CaseBehaviorAction outer)
        {
            Behavior = new WrapBehavior(outer, Behavior);
            return this;
        }

        public CaseBehaviorBuilder SetUpTearDown(MethodAction setUp, MethodAction tearDown)
        {
            return Wrap((@case, instance, inner) =>
            {
                var setUpExceptions = setUp(@case.Method, instance);
                if (setUpExceptions.Any())
                {
                    @case.Exceptions.Add(setUpExceptions);
                    return;
                }

                inner.Execute(@case, instance);

                var tearDownExceptions = tearDown(@case.Method, instance);
                if (tearDownExceptions.Any())
                    @case.Exceptions.Add(tearDownExceptions);
            });
        }

        public CaseBehaviorBuilder SetUpTearDown(MethodFilter setUpMethods, MethodFilter tearDownMethods)
        {
            return SetUpTearDown((method, instance) => setUpMethods.InvokeAll(method.ReflectedType, instance),
                                 (method, instance) => tearDownMethods.InvokeAll(method.ReflectedType, instance));
        }

        class WrapBehavior : CaseBehavior
        {
            readonly CaseBehaviorAction outer;
            readonly CaseBehavior inner;

            public WrapBehavior(CaseBehaviorAction outer, CaseBehavior inner)
            {
                this.outer = outer;
                this.inner = inner;
            }

            public void Execute(Case @case, object instance)
            {
                try
                {
                    outer(@case, instance, inner);
                }
                catch (Exception exception)
                {
                    @case.Exceptions.Add(exception);
                }
            }
        }
    }
}