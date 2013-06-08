using System;
using Fixie.Behaviors;
using Fixie.Conventions;

namespace Fixie.Samples
{
    //The types in this file serve the NUnit and xUnit samples.
    //Once refined, they'll be promoted to members of the Fixie
    //namespace.

    public static class MethodFilterExtensions
    {
        private static ExceptionList InvokeAll(this MethodFilter methodFilter, Type fixtureClass, object instance)
        {
            var invoke = new Invoke();
            var exceptions = new ExceptionList();
            foreach (var method in methodFilter.Filter(fixtureClass))
                invoke.Execute(method, instance, exceptions);
            return exceptions;
        }

        public static MethodBehaviorBuilder SetUpTearDown<TSetUpAttribute, TTearDownAttribute>(this MethodBehaviorBuilder builder)
            where TSetUpAttribute : Attribute
            where TTearDownAttribute : Attribute
        {
            var hasSetUpAttribute = new MethodFilter().HasOrInherits<TSetUpAttribute>();
            var hasTearDownAttribute = new MethodFilter().HasOrInherits<TTearDownAttribute>();

            return builder.SetUpTearDown(hasSetUpAttribute, hasTearDownAttribute);
        }

        public static InstanceBehaviorBuilder SetUpTearDown<TSetUpAttribute, TTearDownAttribute>(this InstanceBehaviorBuilder builder)
            where TSetUpAttribute : Attribute
            where TTearDownAttribute : Attribute
        {
            var hasSetUpAttribute = new MethodFilter().HasOrInherits<TSetUpAttribute>();
            var hasTearDownAttribute = new MethodFilter().HasOrInherits<TTearDownAttribute>();

            return builder.SetUpTearDown(hasSetUpAttribute, hasTearDownAttribute);
        }

        public static MethodBehaviorBuilder SetUpTearDown(this MethodBehaviorBuilder builder, MethodFilter hasSetUpAttribute, MethodFilter hasTearDownAttribute)
        {
            return builder.SetUpTearDown(
                (method, instance) => hasSetUpAttribute.InvokeAll(method.ReflectedType, instance),
                (method, instance) => hasTearDownAttribute.InvokeAll(method.ReflectedType, instance));
        }

        public static InstanceBehaviorBuilder SetUpTearDown(this InstanceBehaviorBuilder builder, MethodFilter hasSetUpAttribute, MethodFilter hasTearDownAttribute)
        {
            return builder.SetUpTearDown(hasSetUpAttribute.InvokeAll, hasTearDownAttribute.InvokeAll);
        }
    }
}