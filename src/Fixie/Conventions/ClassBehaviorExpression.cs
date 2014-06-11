using System;
using System.Collections.Generic;
using Fixie.Behaviors;

namespace Fixie.Conventions
{
    public class ClassBehaviorExpression
    {
        readonly ConfigModel config;
        readonly List<Type> customBehaviors;

        public ClassBehaviorExpression(ConfigModel config)
        {
            this.config = config;
            customBehaviors = new List<Type>();
            OrderCases = executions => { };
        }

        public BehaviorChain<ClassExecution> BuildBehaviorChain()
        {
            var chain = new BehaviorChain<ClassExecution>();

            foreach (var customBehavior in customBehaviors)
                chain.Add((ClassBehavior)Activator.CreateInstance(customBehavior));

            chain.Add(GetInnermostBehavior());

            return chain;
        }

        ClassBehavior GetInnermostBehavior()
        {
            if (config.ConstructionFrequency == ConstructionFrequency.PerCase)
                return new CreateInstancePerCase(config.Factory);

            return new CreateInstancePerClass(config.Factory);
        }

        public Action<Case[]> OrderCases { get; private set; }

        public ClassBehaviorExpression CreateInstancePerCase()
        {
            config.ConstructionFrequency = ConstructionFrequency.PerCase;
            return this;
        }

        public ClassBehaviorExpression CreateInstancePerClass()
        {
            config.ConstructionFrequency = ConstructionFrequency.PerClass;
            return this;
        }

        public ClassBehaviorExpression UsingFactory(Func<Type, object> customFactory)
        {
            config.Factory = customFactory;
            return this;
        }

        public ClassBehaviorExpression Wrap<TClassBehavior>() where TClassBehavior : ClassBehavior
        {
            customBehaviors.Insert(0, typeof(TClassBehavior));
            return this;
        }

        public ClassBehaviorExpression ShuffleCases(Random random)
        {
            OrderCases = caseExecutions => caseExecutions.Shuffle(random);
            return this;
        }

        public ClassBehaviorExpression ShuffleCases()
        {
            return ShuffleCases(new Random());
        }

        public ClassBehaviorExpression SortCases(Comparison<Case> comparison)
        {
            OrderCases = cases => Array.Sort(cases, comparison);
            return this;
        }
    }
}