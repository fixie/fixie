using System;
using Fixie.Behaviors;
using Fixie.Conventions;

namespace Fixie.DSL
{
    public class ClassBehaviorExpression
    {
        readonly ConfigModel config;

        public ClassBehaviorExpression(ConfigModel config)
        {
            this.config = config;
        }

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
            config.TestClassFactory = customFactory;
            return this;
        }

        public ClassBehaviorExpression Wrap<TClassBehavior>() where TClassBehavior : ClassBehavior
        {
            config.WrapClasses<TClassBehavior>();
            return this;
        }

        public ClassBehaviorExpression ShuffleCases(Random random)
        {
            config.OrderCases = caseExecutions => caseExecutions.Shuffle(random);
            return this;
        }

        public ClassBehaviorExpression ShuffleCases()
        {
            return ShuffleCases(new Random());
        }

        public ClassBehaviorExpression SortCases(Comparison<Case> comparison)
        {
            config.OrderCases = cases => Array.Sort(cases, comparison);
            return this;
        }
    }
}