using System;
using Fixie.Behaviors;

namespace Fixie.Conventions
{
    public delegate void TypeBehaviorAction(Type testClass, Convention convention, Case[] cases, Action innerBehavior);
    public delegate void TypeAction(Type testClass);
    public delegate ExceptionList Factory(Type testClass, out object instance);

    public class TypeBehaviorBuilder
    {
        public TypeBehaviorBuilder()
        {
            Behavior = null;
        }

        public TypeBehavior Behavior { get; private set; }

        public TypeBehaviorBuilder CreateInstancePerCase()
        {
            Behavior = new CreateInstancePerCase(Lifecycle.Construct);
            return this;
        }
        public TypeBehaviorBuilder CreateInstancePerCase(Func<Type, object> construct)
        {
            Behavior = new CreateInstancePerCase(new SafeFactory(construct).Construct);
            return this;
        }

        public TypeBehaviorBuilder CreateInstancePerTestClass()
        {
            Behavior = new CreateInstancePerTestClass(Lifecycle.Construct);
            return this;
        }

        public TypeBehaviorBuilder CreateInstancePerTestClass(Func<Type, object> construct)
        {
            Behavior = new CreateInstancePerTestClass(new SafeFactory(construct).Construct);
            return this;
        }

        public TypeBehaviorBuilder Wrap(TypeBehaviorAction outer)
        {
            Behavior = new WrapBehavior(outer, Behavior);
            return this;
        }

        public TypeBehaviorBuilder SetUpTearDown(TypeAction setUp, TypeAction tearDown)
        {
            return Wrap((testClass, convention, cases, innerBehavior) =>
            {
                setUp(testClass);
                innerBehavior();
                tearDown(testClass);
            });
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
                    {
                        @case.Exceptions.Add(exception);
                    }
                }                
            }
        }

        class SafeFactory
        {
            readonly Func<Type, object> construct;

            public SafeFactory(Func<Type, object> construct)
            {
                this.construct = construct;
            }

            public ExceptionList Construct(Type type, out object instance)
            {
                var exceptions = new ExceptionList();

                instance = null;

                try
                {
                    instance = construct(type);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }

                return exceptions;
            }
        }
    }
}