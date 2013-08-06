using System;
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
            return SetUpTearDown(fixture =>
            {
                //NOTE: We want to just throw like normal if InvokeAll ever experiences an exception,
                //      allowing WrapBehavior's own try/catch to handle it correctly.
                //      As is, if the inner behavior doesn't happen to short-cicuit case-running
                //      for cases that have already failed, we could run the case even after
                //      failed setup.
                //
                //      The use of WrappedExceptions is a workaround which only fixes the
                //      immediate issue, but suggests that all exception handling should
                //      be done by throwing a wrapper exception instead of explicitly
                //      passing around ExceptionLists as return values.

                var setUpExceptions = InvokeAll(setUpMethods, fixture.TestClass, fixture.Instance);
                if (setUpExceptions.Any())
                    throw new WrappedExceptions(setUpExceptions);
            },
            fixture =>
            {
                var tearDownExceptions = InvokeAll(tearDownMethods, fixture.TestClass, fixture.Instance);
                if (tearDownExceptions.Any())
                {
                    foreach (var @case in fixture.Cases)
                        @case.Exceptions.Add(tearDownExceptions);
                }
            });
        }

        class WrappedExceptions : Exception
        {
            public WrappedExceptions(ExceptionList innerExceptions)
                : base("See InnerExceptions.")
            {
                InnerExceptions = innerExceptions;
            }

            public ExceptionList InnerExceptions { get; private set; }
        }

        static ExceptionList InvokeAll(MethodFilter methodFilter, Type type, object instance)
        {
            var exceptions = new ExceptionList();
            var invoke = new Invoke();
            foreach (var method in methodFilter.Filter(type))
            {
                invoke.Execute(method, instance, exceptions);
                if (exceptions.Count > 0)
                    break;
            }
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
                catch (WrappedExceptions exceptions)
                {
                    foreach (var @case in fixture.Cases)
                    {
                        @case.Exceptions.Add(exceptions.InnerExceptions);
                    }
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