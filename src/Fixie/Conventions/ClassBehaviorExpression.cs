using System;
using System.Collections.Generic;
using System.Reflection;
using Fixie.Behaviors;

namespace Fixie.Conventions
{
    public class ClassBehaviorExpression
    {
        ConstructionFrequency constructionFrequency;
        Func<Type, object> factory;
        readonly List<Type> customBehaviors;

        public ClassBehaviorExpression()
        {
            constructionFrequency = ConstructionFrequency.PerCase;
            factory = UseDefaultConstructor;
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
            if (constructionFrequency == ConstructionFrequency.PerCase)
                return new CreateInstancePerCase(factory);

            return new CreateInstancePerClass(factory);
        }

        public Action<Case[]> OrderCases { get; private set; }

        public ClassBehaviorExpression CreateInstancePerCase()
        {
            constructionFrequency = ConstructionFrequency.PerCase;
            return this;
        }

        public ClassBehaviorExpression CreateInstancePerClass()
        {
            constructionFrequency = ConstructionFrequency.PerClass;
            return this;
        }

        public ClassBehaviorExpression UsingFactory(Func<Type, object> customFactory)
        {
            factory = customFactory;
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

        static object UseDefaultConstructor(Type type)
        {
            try
            {
                return Activator.CreateInstance(type);
            }
            catch (TargetInvocationException exception)
            {
                throw new PreservedException(exception.InnerException);
            }
        }
    }
}