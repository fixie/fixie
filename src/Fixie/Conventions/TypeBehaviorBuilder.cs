using System;
using Fixie.Behaviors;

namespace Fixie.Conventions
{
    public delegate void TypeBehaviorAction(Type testClass, Convention convention, Case[] cases, TypeBehavior inner);
    public delegate ExceptionList TypeAction(Type testClass);
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
        public TypeBehaviorBuilder CreateInstancePerCase(Factory construct)
        {
            Behavior = new CreateInstancePerCase(new SafeFactory(construct).Construct);
            return this;
        }

        public TypeBehaviorBuilder CreateInstancePerFixture()
        {
            Behavior = new CreateInstancePerFixture(Lifecycle.Construct);
            return this;
        }

        public TypeBehaviorBuilder CreateInstancePerFixture(Factory construct)
        {
            Behavior = new CreateInstancePerFixture(new SafeFactory(construct).Construct);
            return this;
        }

        public TypeBehaviorBuilder Wrap(TypeBehaviorAction outer)
        {
            Behavior = new WrapBehavior(outer, Behavior);
            return this;
        }

        public TypeBehaviorBuilder SetUpTearDown(TypeAction setUp, TypeAction tearDown)
        {
            return Wrap((testClass, convention, cases, inner) =>
            {
                var setUpExceptions = setUp(testClass);
                if (setUpExceptions.Any())
                {
                    foreach (var @case in cases)
                        @case.Exceptions.Add(setUpExceptions);
                    return;
                }

                inner.Execute(testClass, convention, cases);

                var tearDownExceptions = tearDown(testClass);
                if (tearDownExceptions.Any())
                    foreach (var @case in cases)
                        @case.Exceptions.Add(tearDownExceptions);
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
                    outer(testClass, convention, cases, inner);
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
            readonly Factory construct;

            public SafeFactory(Factory construct)
            {
                this.construct = construct;
            }

            public ExceptionList Construct(Type type, out object instance)
            {
                var exceptions = new ExceptionList();

                instance = null;

                try
                {
                    var factoryExceptions = construct(type, out instance);
                    if (factoryExceptions.Any())
                        exceptions.Add(factoryExceptions);
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