using System;
using Fixie.Behaviors;

namespace Fixie.Conventions
{
    public delegate void InstanceBehaviorAction(Fixture fixture, Convention convention, InstanceBehavior inner);
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
            return Wrap((fixture, convention, inner) =>
            {
                var setUpExceptions = setUp(fixture);
                if (setUpExceptions.Any())
                {
                    foreach (var @case in fixture.Cases)
                        @case.Exceptions.Add(setUpExceptions);
                    return;
                }

                inner.Execute(fixture, convention);

                var tearDownExceptions = tearDown(fixture);
                if (tearDownExceptions.Any())
                    foreach (var @case in fixture.Cases)
                        @case.Exceptions.Add(tearDownExceptions);
            });
        }

        public InstanceBehaviorBuilder SetUpTearDown(MethodFilter setUpMethods, MethodFilter tearDownMethods)
        {
            return SetUpTearDown(fixture => setUpMethods.InvokeAll(fixture.Type, fixture.Instance),
                                 fixture => tearDownMethods.InvokeAll(fixture.Type, fixture.Instance));
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

            public void Execute(Fixture fixture, Convention convention)
            {
                try
                {
                    outer(fixture, convention, inner);
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