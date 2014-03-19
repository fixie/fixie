using System;
using Fixie.Behaviors;

namespace Fixie.Conventions
{
    public delegate void InstanceBehaviorAction(TestClassInstance testClassInstance, Action innerBehavior);

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

        public InstanceBehaviorBuilder SetUp(Action<TestClassInstance> setUp)
        {
            return Wrap((testClassInstance, innerBehavior) =>
            {
                setUp(testClassInstance);
                innerBehavior();
            });
        }

        public InstanceBehaviorBuilder SetUpTearDown(Action<TestClassInstance> setUp, Action<TestClassInstance> tearDown)
        {
            return Wrap((testClassInstance, innerBehavior) =>
            {
                setUp(testClassInstance);
                innerBehavior();
                tearDown(testClassInstance);
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

            public void Execute(TestClassInstance testClassInstance)
            {
                try
                {
                    outer(testClassInstance, () => inner.Execute(testClassInstance));
                }
                catch (Exception exception)
                {
                    testClassInstance.FailCases(exception);
                }
            }
        }
    }
}