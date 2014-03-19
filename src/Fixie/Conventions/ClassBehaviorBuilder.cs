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
            Behavior = null;
        }

        public ClassBehavior Behavior { get; private set; }

        public ClassBehaviorBuilder CreateInstancePerCase()
        {
            Behavior = new CreateInstancePerCase(Construct);
            return this;
        }
        public ClassBehaviorBuilder CreateInstancePerCase(Func<Type, object> construct)
        {
            Behavior = new CreateInstancePerCase(construct);
            return this;
        }

        public ClassBehaviorBuilder CreateInstancePerTestClass()
        {
            Behavior = new CreateInstancePerTestClass(Construct);
            return this;
        }

        public ClassBehaviorBuilder CreateInstancePerTestClass(Func<Type, object> construct)
        {
            Behavior = new CreateInstancePerTestClass(construct);
            return this;
        }

        public ClassBehaviorBuilder Wrap(ClassBehaviorAction outer)
        {
            Behavior = new WrapBehavior(outer, Behavior);
            return this;
        }

        public ClassBehaviorBuilder SetUp(Action<Type> setUp)
        {
            return Wrap((classExecution, innerBehavior) =>
            {
                setUp(classExecution.TestClass);
                innerBehavior();
            });
        }

        public ClassBehaviorBuilder SetUpTearDown(Action<Type> setUp, Action<Type> tearDown)
        {
            return Wrap((classExecution, innerBehavior) =>
            {
                setUp(classExecution.TestClass);
                innerBehavior();
                tearDown(classExecution.TestClass);
            });
        }

        public ClassBehaviorBuilder ShuffleCases(Random random)
        {
            return Wrap((classExecution, innerBehavior) =>
            {
                classExecution.ShuffleCases(random);
                innerBehavior();
            });
        }

        public ClassBehaviorBuilder ShuffleCases()
        {
            return ShuffleCases(new Random());
        }

        public ClassBehaviorBuilder SortCases(Comparison<Case> comparison)
        {
            return Wrap((classExecution, innerBehavior) =>
            {
                classExecution.SortCases(comparison);
                innerBehavior();
            });
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