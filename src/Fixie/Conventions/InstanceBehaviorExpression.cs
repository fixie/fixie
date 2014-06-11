using System;
using System.Collections.Generic;
using Fixie.Behaviors;

namespace Fixie.Conventions
{
    public class InstanceBehaviorExpression
    {
        readonly List<Type> customBehaviors = new List<Type>();

        public BehaviorChain<InstanceExecution> BuildBehaviorChain()
        {
            var chain = new BehaviorChain<InstanceExecution>();

            foreach (var customBehavior in customBehaviors)
                chain.Add((InstanceBehavior)Activator.CreateInstance(customBehavior));

            chain.Add(new ExecuteCases());

            return chain;
        }

        public InstanceBehaviorExpression Wrap<TInstanceBehavior>() where TInstanceBehavior : InstanceBehavior
        {
            customBehaviors.Insert(0, typeof(TInstanceBehavior));
            return this;
        }
    }
}