using System;
using System.Reflection;
using Fixie.Behaviors;

namespace Fixie.Conventions
{
    public delegate void InstanceBehaviorAction(Fixture fixture, Action innerBehavior);

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

        public InstanceBehaviorBuilder SetUpTearDown(Action<Fixture> setUp, Action<Fixture> tearDown)
        {
            return Wrap((fixture, innerBehavior) =>
            {
                setUp(fixture);
                innerBehavior();
                tearDown(fixture);
            });
        }

        public InstanceBehaviorBuilder SetUpTearDown(MethodFilter setUpMethods, MethodFilter tearDownMethods)
        {
            return SetUpTearDown(fixture => InvokeAll(setUpMethods, fixture.TestClass, fixture.Instance),
                                 fixture => InvokeAll(tearDownMethods, fixture.TestClass, fixture.Instance));
        }

        static void InvokeAll(MethodFilter methodFilter, Type type, object instance)
        {
            foreach (var method in methodFilter.Filter(type))
            {
                try
                {
                    method.Invoke(instance, null);
                }
                catch (TargetInvocationException ex)
                {
                    throw new PreservedException(ex.InnerException);
                }
            }
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
                catch (PreservedException preservedException)
                {
                    foreach (var @case in fixture.Cases)
                        @case.Exceptions.Add(preservedException.OriginalException);
                }
                catch (Exception exception)
                {
                    foreach (var @case in fixture.Cases)
                        @case.Exceptions.Add(exception);
                }
            }
        }
    }
}