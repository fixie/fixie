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
}
