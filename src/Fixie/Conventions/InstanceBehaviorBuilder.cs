using System;
using Fixie.Behaviors;

namespace Fixie.Conventions
{
    public delegate void InstanceBehaviorAction(Type fixtureClass, object instance, Case[] cases, Convention convention, InstanceBehavior inner);
    public delegate ExceptionList InstanceAction(Type fixtureClass, object instance);

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
            return Wrap((fixtureClass, instance, cases, convention, inner) =>
            {
                var setUpExceptions = setUp(fixtureClass, instance);
                if (setUpExceptions.Any())
                {
                    foreach (var @case in cases)
                        @case.Exceptions.Add(setUpExceptions);
                    return;
                }

                inner.Execute(fixtureClass, instance, cases, convention);

                var tearDownExceptions = tearDown(fixtureClass, instance);
                if (tearDownExceptions.Any())
                    foreach (var @case in cases)
                        @case.Exceptions.Add(tearDownExceptions);
            });
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

            public void Execute(Type fixtureClass, object instance, Case[] cases, Convention convention)
            {
                try
                {
                    outer(fixtureClass, instance, cases, convention, inner);
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