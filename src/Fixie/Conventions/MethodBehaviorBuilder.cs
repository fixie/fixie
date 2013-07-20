using System;
using System.Reflection;
using Fixie.Behaviors;

namespace Fixie.Conventions
{
    public delegate void MethodBehaviorAction(MethodInfo method, object instance, ExceptionList exceptions, CaseBehavior inner);
    public delegate ExceptionList MethodAction(MethodInfo method, object instance);

    public class MethodBehaviorBuilder
    {
        public MethodBehaviorBuilder()
        {
            Behavior = new Invoke();
        }

        public CaseBehavior Behavior { get; private set; }

        public MethodBehaviorBuilder Wrap(MethodBehaviorAction outer)
        {
            Behavior = new WrapBehavior(outer, Behavior);
            return this;
        }

        public MethodBehaviorBuilder SetUpTearDown(MethodAction setUp, MethodAction tearDown)
        {
            return Wrap((method, instance, exceptions, inner) =>
            {
                var setUpExceptions = setUp(method, instance);
                if (setUpExceptions.Any())
                {
                    exceptions.Add(setUpExceptions);
                    return;
                }

                inner.Execute(method, instance, exceptions);

                var tearDownExceptions = tearDown(method, instance);
                if (tearDownExceptions.Any())
                    exceptions.Add(tearDownExceptions);
            });
        }

        public MethodBehaviorBuilder SetUpTearDown(MethodFilter setUpMethods, MethodFilter tearDownMethods)
        {
            return SetUpTearDown((method, instance) => setUpMethods.InvokeAll(method.ReflectedType, instance),
                                 (method, instance) => tearDownMethods.InvokeAll(method.ReflectedType, instance));
        }

        class WrapBehavior : CaseBehavior
        {
            readonly MethodBehaviorAction outer;
            readonly CaseBehavior inner;

            public WrapBehavior(MethodBehaviorAction outer, CaseBehavior inner)
            {
                this.outer = outer;
                this.inner = inner;
            }

            public void Execute(MethodInfo method, object instance, ExceptionList exceptions)
            {
                try
                {
                    outer(method, instance, exceptions, inner);
                }
                catch (Exception exception)
                {
                    exceptions.Add(exception);
                }
            }
        }
    }
}