using System;
using Fixie.Behaviors;
using Fixie.Conventions;

namespace Fixie.Samples
{
    //The types in this file serve the NUnit and xUnit samples.
    //Once refined, they'll be promoted to members of the Fixie
    //namespace.

    public delegate ExceptionList ClassAction(Type testClass);

    public static class MethodFilterExtensions
    {
        public static ExceptionList InvokeAll(this MethodFilter methodFilter, Type testClass, object instance)
        {
            var invoke = new Invoke();
            var exceptions = new ExceptionList();
            foreach (var method in methodFilter.Filter(testClass))
                invoke.Execute(method, instance, exceptions);
            return exceptions;
        }

        public static MethodBehaviorBuilder SetUpTearDown<TSetUpAttribute, TTearDownAttribute>(this MethodBehaviorBuilder builder)
            where TSetUpAttribute : Attribute
            where TTearDownAttribute : Attribute
        {
            return builder.SetUpTearDown(
                (method, instance) => new MethodFilter().HasOrInherits<TSetUpAttribute>().InvokeAll(method.ReflectedType, instance),
                (method, instance) => new MethodFilter().HasOrInherits<TTearDownAttribute>().InvokeAll(method.ReflectedType, instance));
        }

        public static InstanceBehaviorBuilder SetUpTearDown<TSetUpAttribute, TTearDownAttribute>(this InstanceBehaviorBuilder builder)
            where TSetUpAttribute : Attribute
            where TTearDownAttribute : Attribute
        {
            return builder.SetUpTearDown(
                (fixtureClass, instance) => new MethodFilter().HasOrInherits<TSetUpAttribute>().InvokeAll(fixtureClass, instance),
                (fixtureClass, instance) => new MethodFilter().HasOrInherits<TTearDownAttribute>().InvokeAll(fixtureClass, instance));
        }
    }

    public class TypeBehaviorBuilder
    {
        public TypeBehavior Behavior { get; private set; }

        public TypeBehaviorBuilder CreateInstancePerCase()
        {
            Behavior = new CreateInstancePerCase();
            return this;
        }

        public TypeBehaviorBuilder CreateInstancePerFixture()
        {
            Behavior = new CreateInstancePerFixture();
            return this;
        }

        public TypeBehaviorBuilder SetUpTearDown(ClassAction setUp, ClassAction tearDown)
        {
            Behavior = new ClassSetUpTearDown(setUp, Behavior, tearDown);
            return this;
        }

        class ClassSetUpTearDown : TypeBehavior
        {
            readonly ClassAction setUp;
            readonly TypeBehavior inner;
            readonly ClassAction tearDown;

            public ClassSetUpTearDown(ClassAction setUp, TypeBehavior inner, ClassAction tearDown)
            {
                this.setUp = setUp;
                this.inner = inner;
                this.tearDown = tearDown;
            }

            public void Execute(Type fixtureClass, Convention convention, Case[] cases)
            {
                var classSetUpExceptions = setUp(fixtureClass);
                if (classSetUpExceptions.Any())
                {
                    foreach (var @case in cases)
                        @case.Exceptions.Add(classSetUpExceptions);
                    return;
                }

                inner.Execute(fixtureClass, convention, cases);

                var classTearDownExceptions = tearDown(fixtureClass);
                foreach (var @case in cases)
                    @case.Exceptions.Add(classTearDownExceptions);
            }
        }
    }
}
