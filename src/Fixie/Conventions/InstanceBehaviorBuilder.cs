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

        public InstanceBehaviorBuilder SetUp(Action<Fixture> setUp)
        {
            return Wrap((fixture, innerBehavior) =>
            {
                setUp(fixture);
                innerBehavior();
            });
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
                    foreach (var caseExecution in fixture.CaseExecutions)
                        caseExecution.Fail(exception);
                }
            }
        }
    }
}