using System;
using System.Collections.Generic;
using System.Reflection;
using Fixie.Behaviors;

namespace Fixie.Conventions
{
    public delegate void ClassBehaviorAction(ClassExecution classExecution, Action innerBehavior);

    public class ClassBehaviorBuilder
    {
        enum ConstructionFrequency
        {
            PerCase,
            PerClass
        }

        ConstructionFrequency constructionFrequency;
        Func<Type, object> factory;
        readonly List<ClassBehaviorAction> customBehaviors;

        public ClassBehaviorBuilder()
        {
            constructionFrequency = ConstructionFrequency.PerCase;
            factory = UseDefaultConstructor;
            customBehaviors = new List<ClassBehaviorAction>();
            OrderCases = executions => { };
        }

        public ClassBehavior BuildBehavior()
        {
            var behavior = GetInnermostBehavior();

            foreach (var customBehavior in customBehaviors)
                behavior = new WrapBehavior(customBehavior, behavior);

            return behavior;
        }

        ClassBehavior GetInnermostBehavior()
        {
            if (constructionFrequency == ConstructionFrequency.PerCase)
                return new CreateInstancePerCase(factory);

            return new CreateInstancePerClass(factory);
        }

        public Action<Case[]> OrderCases { get; private set; }

        public ClassBehaviorBuilder CreateInstancePerCase()
        {
            constructionFrequency = ConstructionFrequency.PerCase;
            return this;
        }

        public ClassBehaviorBuilder CreateInstancePerClass()
        {
            constructionFrequency = ConstructionFrequency.PerClass;
            return this;
        }

        public ClassBehaviorBuilder UsingFactory(Func<Type, object> customFactory)
        {
            factory = customFactory;
            return this;
        }

        public ClassBehaviorBuilder Wrap(ClassBehaviorAction outer)
        {
            customBehaviors.Add(outer);
            return this;
        }

        public ClassBehaviorBuilder Wrap<TDisposable>() where TDisposable : IDisposable, new()
        {
            return Wrap((classExecution, innerBehavior) =>
            {
                using (new TDisposable())
                    innerBehavior();
            });
        }

        public ClassBehaviorBuilder SetUp(Action<ClassExecution> setUp)
        {
            return Wrap((classExecution, innerBehavior) =>
            {
                setUp(classExecution);
                innerBehavior();
            });
        }

        public ClassBehaviorBuilder SetUpTearDown(Action<ClassExecution> setUp, Action<ClassExecution> tearDown)
        {
            return Wrap((classExecution, innerBehavior) =>
            {
                setUp(classExecution);
                innerBehavior();
                tearDown(classExecution);
            });
        }

        public ClassBehaviorBuilder ShuffleCases(Random random)
        {
            OrderCases = caseExecutions => caseExecutions.Shuffle(random);
            return this;
        }

        public ClassBehaviorBuilder ShuffleCases()
        {
            return ShuffleCases(new Random());
        }

        public ClassBehaviorBuilder SortCases(Comparison<Case> comparison)
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

        class WrapBehavior : ClassBehavior
        {
            readonly ClassBehaviorAction outer;
            readonly ClassBehavior inner;

            public WrapBehavior(ClassBehaviorAction outer, ClassBehavior inner)
            {
                this.outer = outer;
                this.inner = inner;
            }

            public void Execute(ClassExecution classExecution)
            {
                try
                {
                    outer(classExecution, () => inner.Execute(classExecution));
                }
                catch (Exception exception)
                {
                    classExecution.Fail(exception);
                }                
            }
        }
    }
}