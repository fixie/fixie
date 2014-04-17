using System;
using System.Collections.Generic;
using System.Reflection;
using Fixie.Behaviors;

namespace Fixie.Conventions
{
    public delegate void ClassBehaviorAction(ClassExecution classExecution, Action innerBehavior);

    public class ClassBehaviorBuilder
    {
        Func<Type, object> constructor;
        readonly List<ClassBehaviorAction> customBehaviors;

        public ClassBehaviorBuilder()
        {
            constructor = Construct;
            customBehaviors = new List<ClassBehaviorAction>();
            ConstructionFrequency = ConstructionFrequency.CreateInstancePerCase;
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
            if (ConstructionFrequency == ConstructionFrequency.CreateInstancePerCase)
                return new CreateInstancePerCase(constructor);

            return new CreateInstancePerClass(constructor);
        }

        public Action<CaseExecution[]> OrderCases { get; private set; }
        public ConstructionFrequency ConstructionFrequency { get; private set; }

        public ClassBehaviorBuilder CreateInstancePerCase()
        {
            ConstructionFrequency = ConstructionFrequency.CreateInstancePerCase;
            constructor = Construct;
            return this;
        }

        public ClassBehaviorBuilder CreateInstancePerCase(Func<Type, object> construct)
        {
            ConstructionFrequency = ConstructionFrequency.CreateInstancePerCase;
            constructor = construct;
            return this;
        }

        public ClassBehaviorBuilder CreateInstancePerClass()
        {
            ConstructionFrequency = ConstructionFrequency.CreateInstancePerClass;
            constructor = Construct;
            return this;
        }

        public ClassBehaviorBuilder CreateInstancePerClass(Func<Type, object> construct)
        {
            ConstructionFrequency = ConstructionFrequency.CreateInstancePerClass;
            constructor = construct;
            return this;
        }

        public ClassBehaviorBuilder Wrap(ClassBehaviorAction outer)
        {
            customBehaviors.Add(outer);
            return this;
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
            OrderCases = caseExecutions =>
                Array.Sort(caseExecutions, (caseExecutionA, caseExecutionB) => comparison(caseExecutionA.Case, caseExecutionB.Case));

            return this;
        }

        static object Construct(Type type)
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
                    classExecution.FailCases(exception);
                }                
            }
        }
    }
}