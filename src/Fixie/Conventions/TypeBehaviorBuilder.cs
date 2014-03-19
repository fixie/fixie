using System;
using System.Reflection;
using Fixie.Behaviors;

namespace Fixie.Conventions
{
    public delegate void TypeBehaviorAction(ClassExecution classExecution, Action innerBehavior);

    public class TypeBehaviorBuilder
    {
        public TypeBehaviorBuilder()
        {
            Behavior = null;
        }

        public TypeBehavior Behavior { get; private set; }

        public TypeBehaviorBuilder CreateInstancePerCase()
        {
            Behavior = new CreateInstancePerCase(Construct);
            return this;
        }
        public TypeBehaviorBuilder CreateInstancePerCase(Func<Type, object> construct)
        {
            Behavior = new CreateInstancePerCase(construct);
            return this;
        }

        public TypeBehaviorBuilder CreateInstancePerTestClass()
        {
            Behavior = new CreateInstancePerTestClass(Construct);
            return this;
        }

        public TypeBehaviorBuilder CreateInstancePerTestClass(Func<Type, object> construct)
        {
            Behavior = new CreateInstancePerTestClass(construct);
            return this;
        }

        public TypeBehaviorBuilder Wrap(TypeBehaviorAction outer)
        {
            Behavior = new WrapBehavior(outer, Behavior);
            return this;
        }

        public TypeBehaviorBuilder SetUp(Action<Type> setUp)
        {
            return Wrap((classExecution, innerBehavior) =>
            {
                setUp(classExecution.TestClass);
                innerBehavior();
            });
        }

        public TypeBehaviorBuilder SetUpTearDown(Action<Type> setUp, Action<Type> tearDown)
        {
            return Wrap((classExecution, innerBehavior) =>
            {
                setUp(classExecution.TestClass);
                innerBehavior();
                tearDown(classExecution.TestClass);
            });
        }

        public TypeBehaviorBuilder ShuffleCases(Random random)
        {
            return Wrap((classExecution, innerBehavior) =>
            {
                classExecution.ShuffleCases(random);
                innerBehavior();
            });
        }

        public TypeBehaviorBuilder ShuffleCases()
        {
            return ShuffleCases(new Random());
        }

        public TypeBehaviorBuilder SortCases(Comparison<Case> comparison)
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

        class WrapBehavior : TypeBehavior
        {
            readonly TypeBehaviorAction outer;
            readonly TypeBehavior inner;

            public WrapBehavior(TypeBehaviorAction outer, TypeBehavior inner)
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