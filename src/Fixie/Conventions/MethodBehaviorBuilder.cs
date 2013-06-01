using System;
using System.Reflection;
using Fixie.Behaviors;

namespace Fixie.Conventions
{
    public delegate void MethodAction(MethodInfo method, object instance, ExceptionList exceptions, MethodBehavior inner);

    public class MethodBehaviorBuilder
    {
        public MethodBehaviorBuilder()
        {
            Behavior = new Invoke();
        }

        public MethodBehavior Behavior { get; private set; }

        public MethodBehaviorBuilder Wrap(MethodAction outer)
        {
            Behavior = new WrapBehavior(outer, Behavior);
            return this;
        }

        public MethodBehaviorBuilder SetUpTearDown(MethodAction setUp, MethodAction tearDown)
        {
            return Wrap((method, instance, exceptions, inner) =>
            {
                var setUpExceptions = new ExceptionList();
                setUp(method, instance, setUpExceptions, inner);

                if (setUpExceptions.Any())
                {
                    exceptions.Add(setUpExceptions);
                    return;
                }

                inner.Execute(method, instance, exceptions);
                tearDown(method, instance, exceptions, inner);
            });
        }

        class WrapBehavior : MethodBehavior
        {
            readonly MethodAction outer;
            readonly MethodBehavior inner;

            public WrapBehavior(MethodAction outer, MethodBehavior inner)
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