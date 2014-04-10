using System;
using System.Reflection;
using Fixie.Behaviors;

namespace Fixie.Conventions
{
    public delegate void ClassBehaviorAction(ClassExecution classExecution, Action innerBehavior);

    public class ClassBehaviorBuilder
    {
        public ClassBehaviorBuilder()
        {
            ConstructionFrequency = ConstructionFrequency.CreateInstancePerCase;
            Behavior = null;
            OrderCases = executions => { };
        }

        public ClassBehavior Behavior { get; private set; }
        public Action<CaseExecution[]> OrderCases { get; private set; }
        public ConstructionFrequency ConstructionFrequency { get; private set; }

        public ClassBehaviorBuilder CreateInstancePerCase()
        {
            ConstructionFrequency = ConstructionFrequency.CreateInstancePerCase;
            Behavior = new CreateInstancePerCase(Construct);
            return this;
        }

        public ClassBehaviorBuilder CreateInstancePerCase(Func<Type, object> construct)
        {
            ConstructionFrequency = ConstructionFrequency.CreateInstancePerCase;
            Behavior = new CreateInstancePerCase(construct);
            return this;
        }

        public ClassBehaviorBuilder CreateInstancePerTestClass()
        {
            ConstructionFrequency = ConstructionFrequency.CreateInstancePerClass;
            Behavior = new CreateInstancePerTestClass(Construct);
            return this;
        }

        public ClassBehaviorBuilder CreateInstancePerTestClass(Func<Type, object> construct)
        {
            ConstructionFrequency = ConstructionFrequency.CreateInstancePerClass;
            Behavior = new CreateInstancePerTestClass(construct);
            return this;
        }

        public ClassBehaviorBuilder Wrap(ClassBehaviorAction outer)
        {
            Behavior = new WrapBehavior(outer, Behavior);
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