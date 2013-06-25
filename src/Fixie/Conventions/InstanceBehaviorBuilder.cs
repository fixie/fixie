using System;
using Fixie.Behaviors;

namespace Fixie.Conventions
{
    public delegate void InstanceBehaviorAction(Fixture fixture, Case[] cases, Convention convention, InstanceBehavior inner);
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
            return Wrap((fixture, cases, convention, inner) =>
            {
                var setUpExceptions = setUp(fixture);
                if (setUpExceptions.Any())
                {
                    foreach (var @case in cases)
                        @case.Exceptions.Add(setUpExceptions);
                    return;
                }

                inner.Execute(fixture, cases, convention);

                var tearDownExceptions = tearDown(fixture);
                if (tearDownExceptions.Any())
                    foreach (var @case in cases)
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

            public void Execute(Fixture fixture, Case[] cases, Convention convention)
            {
                try
                {
                    outer(fixture, cases, convention, inner);
                }
                catch (Exception exception)
                {
                    foreach (var @case in cases)
                    {
                        @case.Exceptions.Add(exception);
                    }
                }
            }
        }
    }
}