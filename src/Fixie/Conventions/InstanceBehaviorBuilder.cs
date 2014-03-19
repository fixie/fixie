using System;
using Fixie.Behaviors;

namespace Fixie.Conventions
{
    public delegate void InstanceBehaviorAction(InstanceExecution instanceExecution, Action innerBehavior);

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

        public InstanceBehaviorBuilder SetUp(Action<InstanceExecution> setUp)
        {
            return Wrap((instanceExecution, innerBehavior) =>
            {
                setUp(instanceExecution);
                innerBehavior();
            });
        }

        public InstanceBehaviorBuilder SetUpTearDown(Action<InstanceExecution> setUp, Action<InstanceExecution> tearDown)
        {
            return Wrap((instanceExecution, innerBehavior) =>
            {
                setUp(instanceExecution);
                innerBehavior();
                tearDown(instanceExecution);
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

            public void Execute(InstanceExecution instanceExecution)
            {
                try
                {
                    outer(instanceExecution, () => inner.Execute(instanceExecution));
                }
                catch (Exception exception)
                {
                    instanceExecution.FailCases(exception);
                }
            }
        }
    }
}