using System;
using Fixie.Behaviors;

namespace Fixie.Conventions
{
    public delegate void InstanceBehaviorAction(Fixture fixture, Action innerBehavior);
    public delegate ExceptionList InstanceAction(Fixture fixture);

    public class InstanceBehaviorBuilder
    {
        public InstanceBehaviorBuilder()
        {
            Behavior = new ExecuteCases();
        }

        public InstanceBehavior Behavior { get; private set; }

        public InstanceBehaviorBuilder Wrap(InstanceBehaviorAction outer)
        {
            Behavior = new WrapBehavior(outer, Behavior);
            return this;
        }

        public InstanceBehaviorBuilder SetUpTearDown(InstanceAction setUp, InstanceAction tearDown)
        {
            return Wrap((fixture, innerBehavior) =>
            {
                var setUpExceptions = setUp(fixture);
                if (setUpExceptions.Any())
                {
                    foreach (var @case in fixture.Cases)
                        @case.Exceptions.Add(setUpExceptions);
                    return;
                }

                innerBehavior();

                var tearDownExceptions = tearDown(fixture);
                if (tearDownExceptions.Any())
                    foreach (var @case in fixture.Cases)
                        @case.Exceptions.Add(tearDownExceptions);
            });
        }

        public InstanceBehaviorBuilder SetUpTearDown(MethodFilter setUpMethods, MethodFilter tearDownMethods)
        {
            return SetUpTearDown(fixture => InvokeAll(setUpMethods, fixture.TestClass, fixture.Instance),
                                 fixture => InvokeAll(tearDownMethods, fixture.TestClass, fixture.Instance));
        }

        static ExceptionList InvokeAll(MethodFilter methodFilter, Type type, object instance)
        {
            var exceptions = new ExceptionList();
            var invoke = new Invoke();
            foreach (var method in methodFilter.Filter(type))
                invoke.Execute(method, instance, exceptions);
            return exceptions;
        }

        class WrapBehavior : InstanceBehavior
        {
            readonly InstanceBehaviorAction outer;
            readonly InstanceBehavior inner;

            public WrapBehavior(InstanceBehaviorAction outer, InstanceBehavior inner)
            {
                this.outer = outer;
                this.inner = inner;
            }

            public void Execute(Fixture fixture)
            {
                try
                {
                    outer(fixture, () => inner.Execute(fixture));
                }
                catch (Exception exception)
                {
                    foreach (var @case in fixture.Cases)
                    {
                        @case.Exceptions.Add(exception);
                    }
                }
            }
        }
    }
}