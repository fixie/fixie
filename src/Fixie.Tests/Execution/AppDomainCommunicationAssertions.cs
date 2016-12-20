namespace Fixie.Tests.Execution
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Fixie.Execution;
    using Should;

    public static class AppDomainCommunicationAssertions
    {
        const BindingFlags AllMembers =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        public static void ShouldSupportReceivingMessagesFromTheChildAppDomain(this Type listenerType)
        {
            listenerType
                .IsSubclassOf(typeof(LongLivedMarshalByRefObject))
                .ShouldBeTrue(
                    $"{listenerType.Name} must be a {nameof(LongLivedMarshalByRefObject)} " +
                    "so that it can successfully receive messages across the AppDomain boundary.");
        }

        public static void ShouldBeSafeAppDomainCommunicationInterface(this Type crossAppDomainInterfaceType)
        {
            foreach (var method in crossAppDomainInterfaceType.GetMethods())
            {
                if (method.DeclaringType == typeof(object) || method.DeclaringType == typeof(MarshalByRefObject))
                    continue;

                if (method.Name == "InitializeLifetimeService")
                    continue;

                if (!method.IsVoid())
                {
                    IsSafeForAppDomainCommunication(method.ReturnType)
                        .ShouldBeTrue(
                            $"{method.ReturnType} is not an acceptable return type for method {crossAppDomainInterfaceType.FullName}.{method.Name} " +
                            "because it will not successfully cross AppDomain boundaries.");
                }

                foreach (var parameterType in method.GetParameters().Select(x => x.ParameterType))
                {
                    IsSafeForAppDomainCommunication(parameterType)
                        .ShouldBeTrue(
                            $"{parameterType} is not an acceptable parameter type for method {crossAppDomainInterfaceType.FullName}.{method.Name} " +
                            "because it will not successfully cross AppDomain boundaries.");
                }
            }
        }

        static bool IsSafeForAppDomainCommunication(Type type)
        {
            return IsSafeForAppDomainCommunication(type, new HashSet<Type>());
        }

        static bool IsSafeForAppDomainCommunication(Type type, HashSet<Type> visitedTypes)
        {
            //This test is meant to catch *typical* mistakes with AppDomain communication, but does not
            //perfectly vet all possible candiate types:
            //
            //  1. Checking for [Serializable] does not guarantee a type is truly serializable.
            //  2. Not every IReadOnlyList<T> will necessarily serialize, but it is convenient to
            //     allow it as the most likely implementations *are* serializable.
            //  3. Although reflection types like Assembly and Type are serializable, they are
            //     also not acceptable for AppDomain communication because they can cause
            //     assembly load failures at runtime.

            if (type == typeof(Listener))
            {
                //Tests use this routine to vet Listener as a safe remoting *interface*,
                //but it is also used as an *argument* on other remoting interfaces.
                //Because it is the responsibility of the Listener implementation to be
                //a valid MarshalByRefObject, there is nothing left to check for here,
                //so it is assumed to be valid.
                return true;
            }

            visitedTypes.Add(type);

            if (type == typeof(CaseResult))
            {
                return KnownCaseResultImplementations()
                    .All(implementationType => IsSafeForAppDomainCommunication(implementationType, visitedTypes));
            }

            if (type == typeof(object))
                return false;

            if (type == typeof(Type))
                return false;

            if (type.IsInNamespace("System.Reflection"))
                return false;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IReadOnlyList<>))
            {
                if (!IsSafeForAppDomainCommunication(type.GetGenericArguments().Single(), visitedTypes))
                    return false;
            }
            else if (type.IsArray)
            {
                if (!IsSafeForAppDomainCommunication(type.GetElementType(), visitedTypes))
                    return false;
            }
            else if (!type.HasOrInherits<SerializableAttribute>())
            {
                return false;
            }

            if (!type.IsArray && type.IsInNamespace("Fixie"))
            {
                foreach (var property in type.GetProperties(AllMembers))
                {
                    var propertyType = property.PropertyType;

                    if (!visitedTypes.Contains(propertyType) && !IsSafeForAppDomainCommunication(propertyType, visitedTypes))
                        return false;
                }
            }

            return true;
        }

        static IEnumerable<Type> KnownCaseResultImplementations()
        {
            return typeof(CaseResult)
                .Assembly
                .GetTypes()
                .Where(type => typeof(CaseResult).IsAssignableFrom(type) && type.IsClass);
        }
    }
}