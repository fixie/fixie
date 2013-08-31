using System;
using System.Reflection;
using Fixie.Behaviors;

namespace Fixie.Conventions
{
    public delegate void TypeBehaviorAction(Type testClass, Convention convention, Case[] cases, Action innerBehavior);

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

        public TypeBehaviorBuilder SetUpTearDown(Action<Type> setUp, Action<Type> tearDown)
        {
            return Wrap((testClass, convention, cases, innerBehavior) =>
            {
                setUp(testClass);
                innerBehavior();
                tearDown(testClass);
            });
        }

        public TypeBehaviorBuilder ShuffleCases(Random random)
        {
            return Wrap((testClass, convention, cases, innerBehavior) =>
            {
                cases.Shuffle(random);
                innerBehavior();
            });
        }

        public TypeBehaviorBuilder ShuffleCases()
        {
            return ShuffleCases(new Random());
        }

        public TypeBehaviorBuilder SortCases(Comparison<Case> comparison)
        {
            return Wrap((testClass, convention, cases, innerBehavior) =>
            {
                Array.Sort(cases, comparison);
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

            public void Execute(Type testClass, Convention convention, Case[] cases)
            {
                try
                {
                    outer(testClass, convention, cases, () => inner.Execute(testClass, convention, cases));
                }
                catch (Exception exception)
                {
                    foreach (var @case in cases)
                        @case.Fail(exception);
                }                
            }
        }
    }
}