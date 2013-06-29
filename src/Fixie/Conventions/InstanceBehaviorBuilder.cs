using System;
using Fixie.Behaviors;

namespace Fixie.Conventions
{
    public delegate void InstanceBehaviorAction(Fixture fixture, InstanceBehavior inner);
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
            return Wrap((fixture, inner) =>
            {
                var setUpExceptions = setUp(fixture);
                if (setUpExceptions.Any())
                {
                    foreach (var @case in fixture.Cases)
                        @case.Exceptions.Add(setUpExceptions);
                    return;
                }

                inner.Execute(fixture);

                var tearDownExceptions = tearDown(fixture);
                if (tearDownExceptions.Any())
                    foreach (var @case in fixture.Cases)
                        @case.Exceptions.Add(tearDownExceptions);
            });
        }

        public InstanceBehaviorBuilder SetUpTearDown(MethodFilter setUpMethods, MethodFilter tearDownMethods)
        {
            return SetUpTearDown(fixture => setUpMethods.InvokeAll(fixture.TestClass, fixture.Instance),
                                 fixture => tearDownMethods.InvokeAll(fixture.TestClass, fixture.Instance));
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
                    outer(fixture, inner);
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